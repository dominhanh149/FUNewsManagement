let pageNumber = 1;
const pageSize = 10;

const modalEl = document.getElementById("accountModal");
const modal = new bootstrap.Modal(modalEl);

function roleText(role) {
    if (role === 1) return "Staff";
    if (role === 2) return "Lecturer";
    if (role === 0) return "Admin";
    return "Unknown";
}

async function loadAccounts() {
    try {
        const keyword = document.getElementById("keyword").value.trim();
        const roleVal = document.getElementById("role").value;

        const qs = new URLSearchParams();
        qs.set("PageNumber", pageNumber);
        qs.set("PageSize", pageSize);
        if (keyword) qs.set("Keyword", keyword);
        if (roleVal) qs.set("Role", roleVal);

        const data = await api.get(`/fe-api/admin/accounts?${qs.toString()}`);

        console.log("loadAccounts data:", data); // ✅ Debug

        // ✅ Xử lý response đúng cách
        // BE có thể trả: { success, data: { items, totalCount } } hoặc { items, totalCount }
        let paging;
        if (data.data && data.data.items) {
            paging = data.data; // Trường hợp ApiResponse { data: PagingDto }
        } else if (data.items) {
            paging = data; // Trường hợp PagingDto trực tiếp
        } else {
            console.error("Invalid response format:", data);
            paging = { items: [], totalCount: 0, pageNumber: 1, pageSize: 10 };
        }

        const items = paging.items || [];
        const tb = document.getElementById("tbAccounts");

        tb.innerHTML = items.map(x => `
            <tr>
                <td>${x.accountId}</td>
                <td>${x.accountName ?? ""}</td>
                <td>${x.accountEmail ?? ""}</td>
                <td>${roleText(x.accountRole)}</td>
                <td>
                    <button class="btn btn-sm btn-outline-primary me-1" onclick="editAccount(${x.accountId})">Edit</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteAccount(${x.accountId})">Delete</button>
                </td>
            </tr>
        `).join("");

        const total = paging.totalCount ?? items.length;
        document.getElementById("pagingInfo").innerText =
            `Page ${paging.pageNumber ?? pageNumber} | Size ${paging.pageSize ?? pageSize} | Total ${total}`;

        document.getElementById("btnPrev").disabled = pageNumber <= 1;
        document.getElementById("btnNext").disabled = (pageNumber * pageSize) >= total;

    } catch (e) {
        console.error("loadAccounts error:", e);
        alert("Failed to load accounts: " + e.message);
    }
}

function openCreate() {
    document.getElementById("modalTitle").innerText = "Create Account";
    document.getElementById("modalMsg").innerText = "";

    document.getElementById("accountId").value = 0;
    document.getElementById("accountName").value = "";
    document.getElementById("accountEmail").value = "";
    document.getElementById("accountRole").value = "1";
    document.getElementById("accountPassword").value = "";

    modal.show();
}

window.editAccount = async function (id) {
    document.getElementById("modalTitle").innerText = "Edit Account";
    document.getElementById("modalMsg").innerText = "";

    // API mẫu của bạn có GET /api/SystemAccounts/{id:int}
    const acc = await api.get(`/fe-api/admin/accounts/${id}`);

    // nếu trả ApiResponse
    const a = acc.data ? acc.data : acc;

    document.getElementById("accountId").value = a.accountId;
    document.getElementById("accountName").value = a.accountName ?? "";
    document.getElementById("accountEmail").value = a.accountEmail ?? "";
    document.getElementById("accountRole").value = (a.accountRole ?? 1).toString();
    document.getElementById("accountPassword").value = ""; // không auto fill password

    modal.show();
}

async function saveAccount() {
    const id = Number(document.getElementById("accountId").value);
    const name = document.getElementById("accountName").value.trim();
    const email = document.getElementById("accountEmail").value.trim();
    const role = Number(document.getElementById("accountRole").value);
    const password = document.getElementById("accountPassword").value;

    const msg = document.getElementById("modalMsg");
    msg.innerText = "";

    if (!name || !email) {
        msg.innerText = "Name and Email are required.";
        return;
    }
    if (id === 0 && !password) {
        msg.innerText = "Password is required when creating.";
        return;
    }

    // Field names must match SystemAccountSaveDto exactly (PascalCase)
    const body = {
        AccountId: id,
        AccountName: name,
        AccountEmail: email,
        AccountRole: role,
        AccountPassword: password ? password : null
    };

    const btn = document.getElementById("btnSave");
    btn.disabled = true;

    try {
        const res = await api.post("/fe-api/admin/accounts/create-or-edit", body);

        console.log("Save response:", res); // ✅ Debug

        // Hiển thị message
        const message = res?.message || res?.Message || "Saved successfully";
        msg.style.color = "green";
        msg.innerText = message;

        // Đợi 500ms
        await new Promise(resolve => setTimeout(resolve, 500));

        // Đóng modal
        modal.hide();

        // Reset form
        document.getElementById("accountId").value = 0;
        document.getElementById("accountName").value = "";
        document.getElementById("accountEmail").value = "";
        document.getElementById("accountRole").value = "1";
        document.getElementById("accountPassword").value = "";
        msg.innerText = "";
        msg.style.color = "";

        // ✅ Reload với try-catch riêng
        try {
            await loadAccounts();
        } catch (loadError) {
            console.error("Failed to reload accounts:", loadError);
            // Nếu load lỗi, refresh trang
            window.location.reload();
        }

    } catch (e) {
        console.error("Save error:", e);
        msg.style.color = "red";
        msg.innerText = e.message || "Save failed";
    } finally {
        btn.disabled = false;
    }
}

window.deleteAccount = async function (id) {
    if (!confirm(`Delete account #${id}?`)) return;

    try {
        const res = await api.del(`/fe-api/admin/accounts/${id}`);

        if (res.success === false) {
            alert(res.message || "Delete failed");
            return;
        }

        await loadAccounts();
    } catch (e) {
        alert(e.message || "Delete failed");
    }
}

document.getElementById("btnSearch").addEventListener("click", async () => {
    pageNumber = 1;
    await loadAccounts();
});

document.getElementById("btnCreate").addEventListener("click", openCreate);
document.getElementById("btnSave").addEventListener("click", saveAccount);

document.getElementById("btnPrev").addEventListener("click", async () => {
    if (pageNumber > 1) pageNumber--;
    await loadAccounts();
});

document.getElementById("btnNext").addEventListener("click", async () => {
    pageNumber++;
    await loadAccounts();
});

document.addEventListener("DOMContentLoaded", loadAccounts);
