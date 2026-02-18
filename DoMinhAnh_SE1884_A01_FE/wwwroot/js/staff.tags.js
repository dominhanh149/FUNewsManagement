// wwwroot/js/staff.tags.js

const TAG_API = "/fe-api/Tag"; // nếu BE bạn dùng /api/Tags thì đổi ở đây

let pageNumber = 1;
const pageSize = 10;

let lastTotal = null; // nếu BE trả totalCount
let lastItemsCount = 0;
let allTagsCache = []; // lưu danh sách tag để validate trùng tên

const tagModalEl = document.getElementById("tagModal");
const articlesModalEl = document.getElementById("articlesModal");
const tagModal = new bootstrap.Modal(tagModalEl);
const articlesModal = new bootstrap.Modal(articlesModalEl);

function safe(x) { return (x ?? "").toString(); }

// unwrap response dạng ApiResponse / PagingDto / list
function unwrap(res) {
    return res?.data ?? res;
}

// unwrap paging: { items, totalCount, pageNumber, pageSize } hoặc ApiResponse{data:{items...}}
function unwrapPaging(res) {
    const x = unwrap(res);
    if (!x) return null;

    // ApiResponse { data: paging }
    if (x.data?.items) return x.data;

    // PagingDto trực tiếp
    if (x.items) return x;

    // Một số BE trả Items/TotalCount PascalCase
    if (x.Items) return {
        items: x.Items,
        totalCount: x.TotalCount ?? x.totalCount ?? null,
        pageNumber: x.PageNumber ?? x.pageNumber ?? null,
        pageSize: x.PageSize ?? x.pageSize ?? null
    };

    // nếu trả thẳng list
    if (Array.isArray(x)) {
        return { items: x, totalCount: null, pageNumber: null, pageSize: null };
    }

    // nếu ApiResponse trả list ở data
    if (Array.isArray(x.data)) {
        return { items: x.data, totalCount: null, pageNumber: null, pageSize: null };
    }

    return null;
}

function getKeyword() {
    const el = document.getElementById("keyword");
    return (el?.value ?? "").trim();
}

function setPagingInfo() {
    const el = document.getElementById("pagingInfo");
    if (!el) return;

    if (lastTotal != null) {
        const from = (pageNumber - 1) * pageSize + (lastItemsCount > 0 ? 1 : 0);
        const to = (pageNumber - 1) * pageSize + lastItemsCount;
        el.innerText = `Showing ${from}-${to} of ${lastTotal}`;
    } else {
        el.innerText = `Page ${pageNumber} • Items: ${lastItemsCount}`;
    }
}

function renderTagsTable(items) {
    const tb = document.getElementById("tbTags");
    if (!tb) return;

    tb.innerHTML = (items || []).map(t => {
        const id = t.TagID ?? t.TagId ?? t.tagId ?? t.tagID; // ✅ thêm tagID
        const name = t.TagName ?? t.tagName ?? "";
        const note = t.Note ?? t.note ?? "";
        const articles = t.ArticleCount ?? t.articleCount ?? t.articlesCount ?? 0;

        return `
      <tr>
        <td>${safe(id)}</td>
        <td>${safe(name)}</td>
        <td>${safe(note)}</td>
        <td>
          <button class="btn btn-sm btn-outline-primary" onclick="viewArticles(${Number(id)})">
            ${safe(articles)}
          </button>
        </td>
        <td>
          <button class="btn btn-sm btn-warning me-1" onclick="editTag(${Number(id)})">Edit</button>
          <button class="btn btn-sm btn-danger" onclick="deleteTag(${Number(id)})">Delete</button>
        </td>
      </tr>
    `;
    }).join("");
}


async function loadTags() {
    try {
        const keyword = document.getElementById("keyword").value.trim();

        // ✅ Tag search endpoint
        const body = {
            tagName: keyword || null,
            pageNumber,
            pageSize
        };

        const data = await api.post("/fe-api/Tag/search", body);

        console.log("loadTags data:", data); // debug

        // ✅ giống Account: unwrap response
        // BE có thể trả: { success, data: { tags, totalCount, pageNumber, pageSize } }
        // hoặc trả thẳng: { tags, totalCount, pageNumber, pageSize }
        let paging;
        const payload = data.data ?? data;

        if (payload.data && (payload.data.tags || payload.data.Tags)) {
            paging = payload.data; // ApiResponse { data: PaginatedTagResponse }
        } else if (payload.tags || payload.Tags) {
            paging = payload; // PaginatedTagResponse trực tiếp
        } else {
            console.error("Invalid response format:", data);
            paging = { tags: [], totalCount: 0, pageNumber: 1, pageSize };
        }

        const items = paging.tags ?? paging.Tags ?? [];
        allTagsCache = items;
        renderTagsTable(items);

        const total = paging.totalCount ?? paging.TotalCount ?? items.length;

        // ✅ hiển thị info (giống account)
        document.getElementById("pagingInfo").innerText =
            `Page ${paging.pageNumber ?? pageNumber} | Size ${paging.pageSize ?? pageSize} | Total ${total}`;

        // ✅ disable nút giống account
        document.getElementById("btnPrev").disabled = pageNumber <= 1;
        document.getElementById("btnNext").disabled = (pageNumber * pageSize) >= total;

    } catch (e) {
        console.error("loadTags error:", e);
        alert("Failed to load tags: " + (e.message || e));
    }
}

function openCreate() {
    document.getElementById("modalTitle").innerText = "Create Tag";
    document.getElementById("modalMsg").innerText = "";

    document.getElementById("tagId").value = 0;
    document.getElementById("tagName").value = "";
    document.getElementById("tagNote").value = "";

    tagModal.show();
}

window.editTag = async function (id) {
    document.getElementById("modalTitle").innerText = "Edit Tag";
    document.getElementById("modalMsg").innerText = "";

    const res = await api.get(`${TAG_API}/${id}`);
    const t = unwrap(res);

    document.getElementById("tagId").value = t.TagID ?? t.TagId ?? id;
    document.getElementById("tagName").value = t.TagName ?? t.tagName ?? "";
    document.getElementById("tagNote").value = t.Note ?? t.note ?? "";

    tagModal.show();
};

async function saveTag() {
    const id = Number(document.getElementById("tagId").value);
    const name = document.getElementById("tagName").value.trim();
    const note = document.getElementById("tagNote").value.trim();
    const msg = document.getElementById("modalMsg");
    msg.innerText = "";

    if (!name) {
        msg.innerText = "TagName is required.";
        return;
    }
    if (isDuplicateTagName(name, id)) {
        msg.innerText = "TagName already exists.";
        return;
    }
    const btn = document.getElementById("btnSave");
    btn.disabled = true;

    try {
        if (id === 0) {
            const res = await api.post(TAG_API, { TagName: name, Note: note });
            const r = unwrap(res);
            if (r?.success === false) {
                msg.innerText = r.message || "Create failed";
                return;
            }
        } else {
            const res = await api.put(`${TAG_API}/${id}`, { TagID: id, TagName: name, Note: note });
            const r = unwrap(res);
            if (r?.success === false) {
                msg.innerText = r.message || "Update failed";
                return;
            }
        }

        tagModal.hide();

        // cleanup backdrop nếu bootstrap bị kẹt
        setTimeout(() => {
            document.querySelectorAll(".modal-backdrop").forEach(x => x.remove());
            document.body.classList.remove("modal-open");
            document.body.style.removeProperty("padding-right");
        }, 200);

        await loadTags();
    } catch (e) {
        msg.innerText = e?.message || "Save failed";
    } finally {
        btn.disabled = false;
    }
}

window.deleteTag = async function (id) {
    // ✅ FE validate: tag đã gán cho article
    if (isTagInUse(id)) {
        alert("Cannot delete this tag because it is already assigned to one or more articles.");
        return;
    }

    if (!confirm(`Delete tag #${id}?`)) return;

    try {
        const res = await api.del(`${TAG_API}/${id}`);
        const r = unwrap(res);

        if (r?.success === false) {
            alert(r.message || "Cannot delete tag.");
            return;
        }

        await loadTags();

        if (lastItemsCount === 0 && pageNumber > 1) {
            pageNumber--;
            await loadTags();
        }
    } catch (e) {
        alert(e?.message || "Delete failed");
    }
};

const articleModal = new bootstrap.Modal(document.getElementById("articlesModal"));

window.viewArticles = async function (tagId) {
    const tb = document.getElementById("tbArticles");
    tb.innerHTML = `<tr><td colspan="4">Loading...</td></tr>`;

    const res = await api.get(`/fe-api/Tag/${tagId}/articles-list`);
    const payload = res?.data ?? res;

    // ✅ unwrap mọi kiểu: data/Data hoặc trả thẳng array
    let items =
        payload?.data ??
        payload?.Data ??
        payload?.result ??
        payload?.Result ??
        payload;

    // ✅ nếu vẫn không phải array, thử items/Items
    if (!Array.isArray(items)) {
        items = items?.items ?? items?.Items ?? [];
    }

    console.log("articles payload:", payload);
    console.log("articles items:", items);

    if (!items.length) {
        tb.innerHTML = `<tr><td colspan="4" class="text-muted">No articles</td></tr>`;
        articleModal.show();
        return;
    }

    tb.innerHTML = items.map(a => {
        const id = a.newsArticleID ?? a.newsArticleId ?? a.NewsArticleID ?? "";
        const title = a.newsTitle ?? a.NewsTitle ?? "";
        const created = a.createdDate ?? a.CreatedDate ?? "";
        const status = (a.newsStatus ?? a.NewsStatus) === true ? "Active" : "Inactive";

        return `
      <tr>
        <td>${escapeHtml(id)}</td>
        <td>${escapeHtml(title)}</td>
        <td>${escapeHtml(created ? new Date(created).toLocaleString() : "")}</td>
        <td>${status}</td>
      </tr>
    `;
    }).join("");

    articleModal.show();
};


function escapeHtml(s) {
    return String(s ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}


// Events
document.getElementById("btnCreate").addEventListener("click", openCreate);
document.getElementById("btnSave").addEventListener("click", saveTag);

document.getElementById("btnSearch").addEventListener("click", async () => {
    pageNumber = 1;
    await loadTags();
});

document.getElementById("btnPrev").addEventListener("click", async () => {
    if (pageNumber > 1) pageNumber--;
    await loadTags();
});

document.getElementById("btnNext").addEventListener("click", async () => {
    pageNumber++;
    await loadTags();
});


// ✅ Quan trọng: DOMContentLoaded phải truyền function, không truyền promise
document.addEventListener("DOMContentLoaded", () => {
    loadTags();
});
function isDuplicateTagName(name, currentId = 0) {
    const normalized = name.trim().toLowerCase();

    return allTagsCache.some(t => {
        const tId = t.TagID ?? t.TagId ?? t.tagId ?? t.tagID;
        const tName = (t.TagName ?? t.tagName ?? "").trim().toLowerCase();

        // bỏ qua chính nó khi edit
        if (Number(tId) === Number(currentId)) return false;

        return tName === normalized;
    });
}
function isTagInUse(tagId) {
    return allTagsCache.some(t => {
        const id = t.TagID ?? t.TagId ?? t.tagId ?? t.tagID;
        const count = t.ArticleCount ?? t.articleCount ?? t.articlesCount ?? 0;
        return Number(id) === Number(tagId) && Number(count) > 0;
    });
}
