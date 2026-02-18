(function () {
    function getUser() {
        try {
            return JSON.parse(localStorage.getItem("funews_user"));
        } catch {
            return null;
        }
    }

    function applyNavbar() {
        const user = getUser();

        const navLogin = document.getElementById("navLogin");
        const navUser = document.getElementById("navUser");
        const navUserText = document.getElementById("navUserText");
        const navRoleText = document.getElementById("navRoleText");

        // hide all role items first
        document.querySelectorAll("[data-role]").forEach(el => el.classList.add("d-none"));

        if (!user) {
            // Guest
            navLogin?.classList.remove("d-none");
            navUser?.classList.add("d-none");
            return;
        }

        // Logged-in
        navLogin?.classList.add("d-none");
        navUser?.classList.remove("d-none");
        navUserText.textContent = `Hello ${user.accountEmail || "User"}`;
        navRoleText.textContent = `Role: ${user.role || "Unknown"}`;

        // Show role-based menu
        document.querySelectorAll(`[data-role="${user.role}"]`).forEach(el => el.classList.remove("d-none"));
    }

    async function logout() {
        try {
            // gọi logout BE (nếu bạn đã tạo endpoint)
            // POST /api/SystemAccounts/logout
            await api.post("/fe-api/admin/accounts/logout", {});
        } catch { /* ignore */ }

        localStorage.removeItem("funews_user");
        window.location.href = "/";
    }

    document.addEventListener("DOMContentLoaded", () => {
        applyNavbar();

        const btnLogout = document.getElementById("btnLogout");
        if (btnLogout) btnLogout.addEventListener("click", logout);
    });

    // expose to other scripts if needed
    window.funews = { applyNavbar };
})();
