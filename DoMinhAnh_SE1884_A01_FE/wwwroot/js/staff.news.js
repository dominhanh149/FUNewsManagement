const modal = new bootstrap.Modal(document.getElementById("newsModal"));

let allTags = [];

// paging state
let newsPageNumber = 1;
const newsPageSize = 10;

let newsLastTotal = null;
let newsLastItemsCount = 0;

function safe(x) { return (x ?? "").toString(); }

function unwrap(res) {
    return res?.data ?? res;
}

// unwrap paging kiểu: ApiResponse{data:{items...}} OR {items...}
function unwrapNewsPaging(res) {
    const x = unwrap(res);
    if (!x) return null;

    if (x.data?.items) return x.data;      // ApiResponse{data:{items}}
    if (x.items) return x;                 // paging trực tiếp

    // PascalCase fallback
    if (x.Items) return {
        items: x.Items,
        totalCount: x.TotalCount ?? x.totalCount ?? null,
        pageNumber: x.PageNumber ?? x.pageNumber ?? null,
        pageSize: x.PageSize ?? x.pageSize ?? null
    };

    return null;
}

function setNewsPagingInfo() {
    const el = document.getElementById("newsPagingInfo");
    if (!el) return;

    if (newsLastTotal != null) {
        const from = (newsPageNumber - 1) * newsPageSize + (newsLastItemsCount > 0 ? 1 : 0);
        const to = (newsPageNumber - 1) * newsPageSize + newsLastItemsCount;
        el.innerText = `Showing ${from}-${to} of ${newsLastTotal}`;
    } else {
        el.innerText = `Page ${newsPageNumber} • Items: ${newsLastItemsCount}`;
    }
}

function updateNewsPagerButtons() {
    const btnPrev = document.getElementById("btnNewsPrev");
    const btnNext = document.getElementById("btnNewsNext");

    if (btnPrev) btnPrev.disabled = newsPageNumber <= 1;

    if (btnNext) {
        if (newsLastTotal != null) {
            const maxPage = Math.max(1, Math.ceil(newsLastTotal / newsPageSize));
            if (newsPageNumber > maxPage) {
                newsPageNumber = maxPage;
                loadNews();
                return;
            }
            btnNext.disabled = newsPageNumber >= maxPage;
        } else {
            btnNext.disabled = newsLastItemsCount < newsPageSize;
        }
    }
}

async function loadNews() {
    const keyword = document.getElementById("keyword").value.trim();

    const body = {
        title: keyword || null,
        pageNumber: newsPageNumber,   // ✅ paging thật
        pageSize: newsPageSize
    };

    const res = await api.post("/fe-api/staff/news/paging", body);
    const paging = unwrapNewsPaging(res);

    const items = paging?.items ?? [];
    newsLastItemsCount = items.length;
    newsLastTotal = paging?.totalCount ?? paging?.TotalCount ?? null;

    const tb = document.getElementById("tbNews");
    tb.innerHTML = items.map(n => {
        const id = n.newsArticleId ?? n.NewsArticleId ?? n.newsArticleID ?? n.NewsArticleID;

        const title = n.newsTitle ?? n.NewsTitle ?? "";
        const headline = n.headline ?? n.Headline ?? "";
        const source = n.newsSource ?? n.NewsSource ?? "";
        const category = n.categoryName ?? n.CategoryName ?? "";
        const isActive = (n.newsStatus ?? n.NewsStatus) === true;

        return `
      <tr>
        <td>${safe(id)}</td>
        <td>${safe(title)}</td>
        <td title="${safe(headline)}">${safe(headline.length > 60 ? headline.slice(0, 60) + "…" : headline)}</td>
        <td>${safe(source)}</td>
        <td>${safe(category)}</td>
        <td>
          <span class="badge ${isActive ? "bg-success" : "bg-secondary"}">
            ${isActive ? "Active" : "Inactive"}
          </span>
        </td>
        <td>
          <button class="btn btn-sm btn-outline-primary" onclick="editNews('${id}')">Edit</button>
          <button class="btn btn-sm btn-outline-warning" onclick="duplicateNews('${id}')">Duplicate</button>
          <button class="btn btn-sm btn-outline-danger" onclick="deleteNews('${id}')">Delete</button>
        </td>
      </tr>
    `;
    }).join("");

    setNewsPagingInfo();
    updateNewsPagerButtons();
}

async function loadCategories() {
    // ✅ ưu tiên gọi search để lấy list active (tránh GET /api/Category trả format lạ)
    // Nếu BE bạn không có /search thì đổi về GET như bên dưới
    let res;
    try {
        res = await api.post("/fe-api/Category/search", {
            searchTerm: null,
            pageNumber: 1,
            pageSize: 1000
        });
    } catch {
        res = await api.get("/fe-api/Category");
    }

    const payload = res?.data ?? res;

    // ✅ unwrap: ApiResponse{data:{items}} | PagingDto{items} | list
    const paging = payload?.data?.items ? payload.data : (payload?.items ? payload : payload);
    const list = paging?.items ?? paging?.Items ?? paging?.data ?? paging;

    const categories = Array.isArray(list) ? list : [];

    const sel = document.getElementById("categoryId");
    if (!sel) return;

    const active = categories.filter(c => (c.isActive ?? c.IsActive) === true);

    sel.innerHTML = active.map(c => {
        const id = c.categoryId ?? c.CategoryID ?? c.CategoryId ?? c.categoryID ?? c.id ?? c.Id;
        const name = c.categoryName ?? c.CategoryName ?? "";
        return `<option value="${Number(id)}">${name}</option>`;
    }).join("");

    // ✅ nếu không có category active thì báo
    if (!sel.innerHTML) {
        sel.innerHTML = `<option value="">(No active categories)</option>`;
    }
}


async function loadTags(selected = []) {
    const res = await api.get("/fe-api/Tag");
    allTags = unwrap(res) || [];

    const wrap = document.getElementById("tagList");
    wrap.innerHTML = allTags.map(t => {
        const id = t.TagID ?? t.TagId ?? t.tagId ?? t.tagID;
        const name = t.TagName ?? t.tagName ?? "";
        const checked = selected.includes(Number(id)) ? "checked" : "";
        return `
      <label class="border px-2 py-1 rounded">
        <input type="checkbox" value="${Number(id)}" ${checked} />
        ${safe(name)}
      </label>
    `;
    }).join("");
}

function openCreate() {
    document.getElementById("modalTitle").innerText = "Create News";
    document.getElementById("modalMsg").innerText = "";
    document.getElementById("newsId").value = "";
    document.getElementById("newsTitle").value = "";
    document.getElementById("headline").value = "";
    document.getElementById("newsSource").value = "";
    document.getElementById("content").value = "";
    document.getElementById("isActive").checked = true;

    loadCategories();
    loadTags([]);
    modal.show();
}

window.editNews = async function (id) {
    const res = await api.get(`/fe-api/staff/news/${id}`);
    const n = unwrap(res);

    document.getElementById("modalTitle").innerText = "Edit News";
    document.getElementById("newsId").value =
        n.newsArticleId ?? n.NewsArticleId ?? n.newsArticleID ?? n.NewsArticleID ?? "";

    document.getElementById("newsTitle").value = n.newsTitle ?? n.NewsTitle ?? "";
    document.getElementById("headline").value = n.headline ?? n.Headline ?? "";
    document.getElementById("newsSource").value = n.newsSource ?? n.NewsSource ?? "";

    const contentEl = document.getElementById("content");
    if (contentEl) contentEl.value = n.newsContent ?? n.NewsContent ?? "";

    document.getElementById("isActive").checked = !!(n.newsStatus ?? n.NewsStatus);

    await loadCategories();

    const selectedTagIds =
        (n.tagIds ?? n.TagIds ?? []).length
            ? (n.tagIds ?? n.TagIds ?? [])
            : (n.tags ?? n.Tags ?? []).map(t => t.tagId ?? t.TagId ?? t.TagID ?? t.tagID);

    await loadTags((selectedTagIds ?? []).map(Number));

    modal.show();
};

async function saveNews() {
    const id = document.getElementById("newsId").value;

    const body = {
        NewsArticleID: id || null,
        NewsTitle: document.getElementById("newsTitle").value,
        Headline: document.getElementById("headline").value,
        NewsSource: document.getElementById("newsSource").value,
        NewsContent: document.getElementById("content").value,
        CategoryID: Number(document.getElementById("categoryId").value),
        NewsStatus: document.getElementById("isActive").checked,
        TagIds: [...document.querySelectorAll("#tagList input:checked")].map(x => Number(x.value))
    };

    try {
        await api.post("/fe-api/staff/news/create-or-edit", body);
        modal.hide();
        await loadNews();
    } catch (e) {
        document.getElementById("modalMsg").innerText = e.message;
    }
}

window.deleteNews = async function (id) {
    if (!confirm("Delete this article?")) return;
    await api.del(`/fe-api/staff/news/${id}`);

    // nếu xóa xong trang rỗng thì lùi page
    await loadNews();
    if (newsLastItemsCount === 0 && newsPageNumber > 1) {
        newsPageNumber--;
        await loadNews();
    }
};

window.duplicateNews = async function (id) {
    await api.post(`/fe-api/staff/news/${id}/duplicate`, {});
    await loadNews();
};

// Events
document.getElementById("btnCreate").onclick = openCreate;
document.getElementById("btnSave").onclick = saveNews;

document.getElementById("btnSearch").onclick = () => {
    newsPageNumber = 1;
    loadNews();
};

document.getElementById("btnNewsPrev")?.addEventListener("click", async () => {
    if (newsPageNumber > 1) newsPageNumber--;
    await loadNews();
});

document.getElementById("btnNewsNext")?.addEventListener("click", async () => {
    newsPageNumber++;
    await loadNews();
});

document.addEventListener("DOMContentLoaded", () => {
    loadNews();
});
