let pageNumber = 1;
const pageSize = 10;
let isBusy = false;

const modalEl = document.getElementById("categoryModal");
const modal = new bootstrap.Modal(modalEl);

function safeText(x) { return (x ?? "").toString(); }
function setToggleButtonsDisabled(disabled) {
    document
        .querySelectorAll('button.btn-outline-warning')
        .forEach(btn => btn.disabled = disabled);
}

async function loadCategories() {
    const keyword = document.getElementById("keyword").value.trim();

    const body = {
        searchTerm: keyword || null,
        pageNumber,
        pageSize
    };

    let res = await api.post("/fe-api/Category/search", body);

    // 🔍 DEBUG: Log toàn bộ response để xem cấu trúc
    console.log("Full API Response:", res);

    const paging = res.items ? res : res.data;
    console.log("Paging object:", paging);

    const items = paging.items || [];
    console.log("Items array:", items);

    // 🔍 DEBUG: Log item đầu tiên để xem tên các thuộc tính
    if (items.length > 0) {
        console.log("First item structure:", items[0]);
        console.log("Keys in first item:", Object.keys(items[0]));
    }

    const total = paging.totalCount ?? items.length;

    // Render rows - thử cả hai dạng viết hoa/thường
    const tb = document.getElementById("tbCategories");
    tb.innerHTML = items.map(c => {
        // 🔍 Tìm ID từ nhiều nguồn có thể
        const categoryId = c.CategoryID || c.categoryID || c.categoryId || c.id || c.Id;

        console.log(`Category ID found: ${categoryId} from object:`, c);

        return `
        <tr>
          <td>${categoryId}</td>
          <td>${safeText(c.categoryName || c.CategoryName)}</td>
          <td>${safeText(c.categoryDescription || c.CategoryDescription || c.categoryDesciption)}</td>
          <td>
            <span class="badge ${c.isActive || c.IsActive ? "bg-success" : "bg-secondary"}">
              ${c.isActive || c.IsActive ? "Active" : "Inactive"}
            </span>
          </td>
          <td id="ac_${categoryId}">...</td>
          <td>
            <button class="btn btn-sm btn-outline-primary me-1" onclick="editCategory(${categoryId})">Edit</button>
            <button class="btn btn-sm btn-outline-danger me-1" onclick="deleteCategory(${categoryId})">Delete</button>
            <button class="btn btn-sm btn-outline-warning" onclick="toggleStatus(${categoryId}, ${!!(c.isActive ?? c.IsActive)})">
              Toggle
            </button>
          </td>
        </tr>
      `;
    }).join("");

    document.getElementById("pagingInfo").innerText =
        `Page ${paging.pageNumber ?? pageNumber} | Size ${paging.pageSize ?? pageSize} | Total ${total}`;

    document.getElementById("btnPrev").disabled = pageNumber <= 1;
    document.getElementById("btnNext").disabled = (pageNumber * pageSize) >= total;

    // Lấy danh sách IDs với xử lý nhiều trường hợp
    const categoryIds = items.map(x => x.CategoryID || x.categoryID || x.categoryId || x.id || x.Id).filter(id => id != null);

    console.log("Category IDs to load counts:", categoryIds);

    await loadArticleCounts(categoryIds);
}

async function loadArticleCounts(ids) {
    console.log("Loading article counts for IDs:", ids);

    await Promise.all(ids.map(async (id) => {
        try {
            console.log(`Fetching article count for category ${id}`);
            const res = await api.get(`/fe-api/Category/${id}/article-count`);
            console.log(`Response for category ${id}:`, res);

            const data = res.data ?? res;

            const count = typeof data === "number" ? data : (data.count ?? data.articleCount ?? 0);

            const cell = document.getElementById(`ac_${id}`);
            if (cell) {
                cell.innerText = count;
                console.log(`Updated cell ac_${id} with count: ${count}`);
            } else {
                console.warn(`Cell ac_${id} not found in DOM`);
            }
        } catch (err) {
            console.error(`Error loading article count for category ${id}:`, err);
            const cell = document.getElementById(`ac_${id}`);
            if (cell) cell.innerText = "-";
        }
    }));
}

function openCreate() {
    document.getElementById("modalTitle").innerText = "Create Category";
    document.getElementById("modalMsg").innerText = "";

    document.getElementById("categoryId").value = 0;
    document.getElementById("categoryName").value = "";
    document.getElementById("categoryDesc").value = "";
    document.getElementById("parentId").value = "";
    document.getElementById("isActive").checked = true;

    modal.show();
}

window.editCategory = async function (id) {
    console.log("Editing category:", id);
    document.getElementById("modalTitle").innerText = "Edit Category";
    document.getElementById("modalMsg").innerText = "";

    const res = await api.get(`/fe-api/Category/${id}`);
    console.log("Edit response:", res);

    const c = res.data ?? res;
    const parentVal = (c.parentCategoryId ?? c.ParentCategoryId ?? "");
    const parentInput = document.getElementById("parentId");
    parentInput.value = parentVal === null ? "" : parentVal;
    parentInput.dataset.original = parentVal === null ? "" : parentVal;
    const countRes = await api.get(`/fe-api/Category/${id}/article-count`);
    const count = countRes.data ?? countRes;
    document.getElementById("parentId").disabled = (Number(count) > 0);
    const categoryId = c.CategoryID || c.categoryID || c.categoryId || c.id || c.Id;

    document.getElementById("categoryId").value = categoryId;
    document.getElementById("categoryName").value = c.categoryName || c.CategoryName || "";
    document.getElementById("categoryDesc").value = c.categoryDescription || c.CategoryDescription || c.categoryDesciption || "";
    document.getElementById("parentId").value = c.parentCategoryId || c.ParentCategoryId || "";
    document.getElementById("parentId").dataset.original = (c.parentCategoryId ?? c.ParentCategoryId ?? "");
    document.getElementById("isActive").checked = !!(c.isActive || c.IsActive);

    modal.show();
}

async function saveCategory() {
    const id = Number(document.getElementById("categoryId").value);
    const name = document.getElementById("categoryName").value.trim();
    const desc = document.getElementById("categoryDesc").value.trim();
    const parentRaw = document.getElementById("parentId").value;
    const isActive = document.getElementById("isActive").checked;
    const parentInput = document.getElementById("parentId");
    const originalParentRaw = (parentInput.dataset.original ?? "").toString();

    let parentCategoryIdToSend;
    const msg = document.getElementById("modalMsg");
    msg.innerText = "";

    if (!name || !desc) {
        msg.innerText = "CategoryName and CategoryDescription are required.";
        return;
    }
    if (parentRaw === "") {
        parentCategoryIdToSend = originalParentRaw === "" ? null : Number(originalParentRaw);
    } else {
        parentCategoryIdToSend = Number(parentRaw);
    }

    const body = {
        categoryId: id,
        categoryName: name,
        categoryDesciption: desc,
        categoryDescription: desc,
        parentCategoryId: parentCategoryIdToSend,
        isActive: isActive
    };

    console.log("Saving category:", body);

    const btn = document.getElementById("btnSave");
    btn.disabled = true;

    try {
        if (id === 0) {
            const res = await api.post("/fe-api/Category", body);
            console.log("Create response:", res);
            if (res.success === false) {
                msg.innerText = res.message || "Create failed";
                return;
            }
        } else {
            const res = await api.put(`/fe-api/Category/${id}`, body);
            console.log("Update response:", res);
            if (res.success === false) {
                msg.innerText = res.message || "Update failed";
                return;
            }
        }

        modal.hide();

        setTimeout(() => {
            document.querySelectorAll(".modal-backdrop").forEach(x => x.remove());
            document.body.classList.remove("modal-open");
            document.body.style.removeProperty("padding-right");
        }, 200);

        await loadCategories();
    } catch (e) {
        console.error("Save error:", e);
        msg.innerText = e.message || "Save failed";
    } finally {
        btn.disabled = false;
    }
}

window.deleteCategory = async function (id) {
    // ✅ FE validate: category đã có article
    if (isCategoryInUse(id)) {
        alert("Cannot delete this category because it already has articles.");
        return;
    }

    if (!confirm(`Delete category #${id}?`)) return;

    try {
        const res = await api.del(`/fe-api/Category/${id}`);
        if (res.success === false) {
            alert(res.message || "Delete failed (category may have articles).");
            return;
        }
        await loadCategories();
    } catch (e) {
        alert(e.message || "Delete failed");
    }
};


window.toggleStatus = async function (categoryId, currentIsActive) {
    if (isBusy) return;               // ⛔ chặn click chồng
    isBusy = true;
    setToggleButtonsDisabled(true);

    try {
        const newIsActive = !currentIsActive;

        console.log(
            "Toggle",
            categoryId,
            "current:",
            currentIsActive,
            "→ new:",
            newIsActive
        );

        const payload = { categoryId, isActive: newIsActive };

        if (api?.request && typeof api.request === "function") {
            await api.request("/fe-api/Category/toggle-status", {
                method: "PATCH",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload),
            });
        } else {
            await fetchToggle(categoryId, newIsActive); // dùng fetch chuẩn của bạn
        }


        await loadCategories();       // ⏳ chờ load xong mới mở khóa
    } catch (e) {
        console.error(e);
        alert(e.message || "Toggle failed");
    } finally {
        isBusy = false;
        setToggleButtonsDisabled(false);
    }
};



async function fetchToggle(categoryId, isActive) {
    const baseUrl = document.querySelector('meta[name="fe-api"]').content;
    const r = await fetch(`${baseUrl}/fe-api/Category/toggle-status`, {
        method: "PATCH",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ categoryId, isActive }),
        credentials: "include"
    });
    const t = await r.text();
    let j = null;
    try { j = t ? JSON.parse(t) : null; } catch { j = t; }
    if (!r.ok) throw new Error((j && (j.message || j.Message)) || r.statusText);
    return j;
}



document.getElementById("btnCreate").onclick = openCreate;
document.getElementById("btnSave").onclick = saveCategory;

document.getElementById("btnSearch").onclick = async () => {
    pageNumber = 1;
    await loadCategories();
};

document.getElementById("btnPrev").onclick = async () => {
    if (pageNumber > 1) pageNumber--;
    await loadCategories();
};

document.getElementById("btnNext").onclick = async () => {
    pageNumber++;
    await loadCategories();
};

document.addEventListener("DOMContentLoaded", loadCategories);
document.getElementById("parentId").addEventListener("input", (e) => {
    const v = e.target.value;
    if (v === "") return;
    const n = Number(v);
    if (!Number.isFinite(n) || n < 0) e.target.value = "0";
});
function isCategoryInUse(categoryId) {
    const cell = document.getElementById(`ac_${categoryId}`);
    if (!cell) return false;

    const count = Number(cell.innerText);
    return Number.isFinite(count) && count > 0;
}
