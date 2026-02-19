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

    // Offline Mode Logic
    function handleConnectionable() {
        const banner = document.getElementById("offlineBanner");
        if (!banner) return;

        if (navigator.onLine) {
            banner.classList.add("d-none");
            // Optional: Show "Back Online" toast if coming from offline
        } else {
            banner.classList.remove("d-none");
        }
    }

    window.addEventListener('online', () => {
        handleConnectionable();
        // Show toast using existing showToast if available (from signalr-client or global)
        // We can dispatch a custom event or check for global function
        const toastContainer = document.getElementById("toastContainer");
        if(toastContainer) {
            // Manual toast creation or reusing signalr's logic if exposed
            // For now, simple console log
            console.log("Back Online");
        }
    });

    window.addEventListener('offline', () => {
        handleConnectionable();
        console.log("Went Offline");
    });

    document.addEventListener("DOMContentLoaded", () => {
        applyNavbar();
        handleConnectionable(); // check on load

        // Register Service Worker
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/service-worker.js')
                .then(reg => console.log('SW registered!', reg))
                .catch(err => console.log('SW failed', err));
        }

        const btnLogout = document.getElementById("btnLogout");
        if (btnLogout) btnLogout.addEventListener("click", logout);
    });

    // expose to other scripts if needed
    window.funews = { applyNavbar };
})();
