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

// ✅ Cache key cố định — Offline Mode
const NEWS_OFFLINE_CACHE_KEY = 'funews_offline_news_paging';

// Ẩn/hiện toàn bộ nút khi Offline Mode
function setOfflineMode(isOffline) {
    // Ẩn nút Create và Search
    const btnCreate = document.getElementById('btnCreate');
    const btnSearch = document.getElementById('btnSearch');
    if (btnCreate) btnCreate.style.display = isOffline ? 'none' : '';
    if (btnSearch) btnSearch.style.display = isOffline ? 'none' : '';

    // Hiện/ẩn banner offline
    let banner = document.getElementById('offlineModeBar');
    if (isOffline && !banner) {
        banner = document.createElement('div');
        banner.id = 'offlineModeBar';
        banner.className = 'alert alert-warning d-flex align-items-center gap-2 mb-3 py-2';
        banner.innerHTML = `
            <i class="bi bi-database-fill-slash"></i>
            <span><strong>Offline Mode</strong> — Đang hiển thị dữ liệu cache.</span>`;
        const h3 = document.querySelector('h3');
        if (h3) h3.insertAdjacentElement('afterend', banner);
    } else if (!isOffline && banner) {
        banner.remove();
    }
}

async function loadNews() {
    const keyword = document.getElementById("keyword").value.trim();

    const body = {
        title: keyword || null,
        pageNumber: newsPageNumber,
        pageSize: newsPageSize
    };

    // ✅ BƯỚC 1: Hiện cache NGAY LẬP TỨC (0ms) trước khi gọi API
    // → Người dùng thấy data ngay, không cần đợi network
    let hasCache = false;
    if (newsPageNumber === 1 && !keyword) {
        try {
            const raw = localStorage.getItem(NEWS_OFFLINE_CACHE_KEY);
            if (raw) {
                const cached = JSON.parse(raw);
                const items = cached.items ?? [];
                if (items.length > 0) {
                    newsLastItemsCount = items.length;
                    newsLastTotal = cached.totalCount ?? null;
                    renderNewsTable(items, false); // Hiện với đầy đủ buttons (tạm thời)
                    setNewsPagingInfo();
                    updateNewsPagerButtons();
                    hasCache = true;
                }
            }
        } catch { /* ignore */ }
    }

    // ✅ BƯỚC 2: Gọi API ngầm để lấy data mới nhất
    try {
        const res = await api.post("/fe-api/staff/news/paging", body, { background: true });
        const paging = unwrapNewsPaging(res);
        const items = paging?.items ?? [];

        newsLastItemsCount = items.length;
        newsLastTotal = paging?.totalCount ?? paging?.TotalCount ?? null;

        // Cập nhật cache với data mới
        if (newsPageNumber === 1 && !keyword && items.length > 0) {
            try {
                localStorage.setItem(NEWS_OFFLINE_CACHE_KEY, JSON.stringify({
                    ts: Date.now(),
                    items,
                    totalCount: newsLastTotal
                }));
            } catch { /* ignore */ }
        }

        // Thay thế bảng bằng data mới nhất (online mode)
        setOfflineMode(false);
        renderNewsTable(items, false);
        setNewsPagingInfo();
        updateNewsPagerButtons();

    } catch (err) {
        // API lỗi → chuyển sang Offline Mode
        if (hasCache) {
            // Đã hiện cache rồi → chỉ cần chuyển sang read-only
            const raw = localStorage.getItem(NEWS_OFFLINE_CACHE_KEY);
            const cached = JSON.parse(raw);
            setOfflineMode(true);
            renderNewsTable(cached.items ?? [], true);
            setNewsPagingInfo();
            updateNewsPagerButtons();
            if (typeof window.showToast === 'function')
                window.showToast('📦 Đang hiển thị dữ liệu cache (Offline Mode)', 'warning');
        } else {
            // Không có cache → hiện thông báo lỗi
            const tb = document.getElementById("tbNews");
            tb.innerHTML = `
                <tr>
                    <td colspan="8" class="text-center text-danger py-4">
                        <i class="bi bi-exclamation-triangle me-2"></i>
                        Không thể tải dữ liệu. Vui lòng kiểm tra kết nối và thử lại.
                    </td>
                </tr>`;
        }
    }
}


function renderNewsTable(items, isOffline = false) {
    const tb = document.getElementById("tbNews");
    tb.innerHTML = items.map(n => {
        const id = n.newsArticleId ?? n.NewsArticleId ?? n.newsArticleID ?? n.NewsArticleID;
        const title = n.newsTitle ?? n.NewsTitle ?? "";
        const headline = n.headline ?? n.Headline ?? "";
        const source = n.newsSource ?? n.NewsSource ?? "";
        const category = n.categoryName ?? n.CategoryName ?? "";
        const isActive = (n.newsStatus ?? n.NewsStatus) === true;
        const imgUrl = n.imageUrl ?? n.ImageUrl ?? "";
        const imgHtml = imgUrl
            ? `<img src="${imgUrl}" style="width:100px; height:60px; object-fit:cover; border-radius:4px" />`
            : `<span class="text-muted small">No Image</span>`;

        // Khi offline: ẩn cột Actions hoàn toàn
        const actionsHtml = isOffline ? '' : `
            <td>
              <button class="btn btn-sm btn-outline-primary" onclick="editNews('${id}')">Edit</button>
              <button class="btn btn-sm btn-outline-warning" onclick="duplicateNews('${id}')">Duplicate</button>
              <button class="btn btn-sm btn-outline-danger" onclick="deleteNews('${id}')">Delete</button>
            </td>`;

        return `
          <tr>
            <td>${safe(id)}</td>
            <td>${imgHtml}</td>
            <td>${safe(title)}</td>
            <td title="${safe(headline)}">${safe(headline.length > 60 ? headline.slice(0, 60) + "…" : headline)}</td>
            <td>${safe(source)}</td>
            <td>${safe(category)}</td>
            <td>
              <span class="badge ${isActive ? "bg-success" : "bg-secondary"}">
                ${isActive ? "Active" : "Inactive"}
              </span>
            </td>
            ${actionsHtml}
          </tr>`;
    }).join("");
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
    document.getElementById("imgPreview").innerHTML = ""; // Clear preview
    document.getElementById("imageUrl").value = "";
    document.getElementById("isActive").checked = true;
    
    loadCategories();
    loadTags([]);
    modal.show();
}

window.editNews = async function (id) {
    clearAiSuggestions();                                      // ← reset AI chips
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
    document.getElementById("imgPreview").innerHTML = ""; // Clear preview

    const imgUrl = n.imageUrl ?? n.ImageUrl ?? "";
    document.getElementById("imageUrl").value = imgUrl;

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
        ImageUrl: document.getElementById("imageUrl").value,
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

// ─── AI Tag Suggestion ────────────────────────────────────────────────────────

// Gợi ý tags dựa trên nội dung bài (gọi AI API)
window.suggestTags = async function () {
    const content = (document.getElementById("content")?.value || "") +
                    " " + (document.getElementById("newsTitle")?.value || "") +
                    " " + (document.getElementById("headline")?.value || "");

    if (content.trim().length < 10) {
        document.getElementById("aiTagSuggestions").innerHTML =
            `<small class="text-warning">Hãy nhập nội dung bài viết trước.</small>`;
        return;
    }

    const btn = document.getElementById("btnSuggestTags");
    const spinner = document.getElementById("suggestSpinner");
    btn.disabled = true;
    spinner.classList.remove("d-none");
    document.getElementById("aiTagSuggestions").innerHTML =
        `<small class="text-muted">Đang phân tích...</small>`;

    try {
        const res = await api.post("/fe-api/ai/suggest-tags", { content });
        const tags = res?.data?.tags ?? res?.tags ?? [];

        if (!tags.length) {
            document.getElementById("aiTagSuggestions").innerHTML =
                `<small class="text-muted">Không tìm thấy tag phù hợp.</small>`;
            return;
        }

        // Render chips
        document.getElementById("aiTagSuggestions").innerHTML = tags.map(t => {
            const pct = Math.round((t.confidence ?? 0) * 100);
            const color = pct >= 70 ? "#6c63ff" : pct >= 40 ? "#4facfe" : "#adb5bd";
            return `
              <span class="badge rounded-pill px-3 py-2 ai-tag-chip"
                    data-tag="${t.tag}"
                    style="background:${color};cursor:pointer;font-size:.82rem;
                           border:2px solid transparent;transition:all .2s"
                    title="${pct}% confidence"
                    onclick="selectAiTag(this, '${t.tag}')">
                  ${t.tag} <span style="opacity:.75;font-size:.7rem">${pct}%</span>
              </span>`;
        }).join("");

    } catch (e) {
        document.getElementById("aiTagSuggestions").innerHTML =
            `<small class="text-danger">Lỗi: ${e.message}</small>`;
    } finally {
        btn.disabled = false;
        spinner.classList.add("d-none");
    }
};

// Khi click vào chip: tìm checkbox tag có tên tương ứng → check tự động
window.selectAiTag = function (el, tagName) {
    const name = tagName.toLowerCase().trim();

    // Tìm checkbox trong tagList có label khớp (case-insensitive)
    const labels = document.querySelectorAll("#tagList label");
    let matched = false;

    for (const label of labels) {
        const labelText = label.textContent.trim().toLowerCase();
        if (labelText.includes(name)) {
            const cb = label.querySelector("input[type='checkbox']");
            if (cb && !cb.checked) {
                cb.checked = true;
                label.style.background = "#e8f5e9";
                label.style.borderColor = "#81c784";
            }
            matched = true;
            break;
        }
    }

    // Visual feedback trên chip
    el.style.outline = "2px solid #fff";
    el.style.opacity = matched ? "0.6" : "1";
    el.title = matched ? "✓ Đã chọn" : "Tag này chưa có trong danh sách";

    // Ghi nhận vào learning cache (fire & forget)
    api.post("/fe-api/ai/learn", { selectedTags: [tagName] }).catch(() => {});
};

// Xoá gợi ý khi mở modal mới
function clearAiSuggestions() {
    const el = document.getElementById("aiTagSuggestions");
    if (el) el.innerHTML = `<small class="text-muted">Nhập nội dung bài viết rồi nhấn "Gợi ý Tag".</small>`;
}

// Events
document.getElementById("btnCreate").onclick = () => { clearAiSuggestions(); openCreate(); };
document.getElementById("btnSave").onclick = saveNews;

document.getElementById("btnSearch").onclick = () => {
    newsPageNumber = 1;
    loadNews();
};

// ─── Image Upload ─────────────────────────────────────────────────────────────
document.getElementById("btnUploadImg").onclick = async function () {
    const input = document.getElementById("imgUpload");
    if (!input.files || !input.files.length) {
        alert("Please select an image file first.");
        return;
    }
    const file = input.files[0];
    const formData = new FormData();
    formData.append("file", file);

    const btn = document.getElementById("btnUploadImg");
    const oldText = btn.innerText;
    btn.innerText = "Up...";
    btn.disabled = true;

    try {
        const res = await api.post("/api/upload/image", formData);
        const url = unwrap(res)?.url ?? res.url;
        
        if (url) {
             const contentArea = document.getElementById("content");
             contentArea.value += `\n![${file.name}](${url})\n`;
             input.value = ""; 

             // Nếu bài viết chưa có ImageUrl (ảnh bìa), tự động lấy ảnh content đầu tiên làm ảnh bìa luôn
             const imgUrlEl = document.getElementById("imageUrl");
             if (!imgUrlEl.value) {
                 imgUrlEl.value = url;
             }
             
             // Show preview
             document.getElementById("imgPreview").innerHTML = 
                `<img src="${url}" class="img-thumbnail" style="max-height:150px" title="Uploaded Image" />`;
        } else {
             alert("Upload successful but no URL returned.");
        }
    } catch (e) {
        console.error(e);
        alert("Upload failed: " + e.message);
    } finally {
        btn.innerText = oldText;
        btn.disabled = false;
    }
};

// Optionally remove btnUploadImg button click entirely, and use purely change event:
document.getElementById("imgUpload").addEventListener("change", async function() {
    if (this.files && this.files.length > 0) {
        document.getElementById("btnUploadImg").click(); // Trigger upload
    }
});

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

