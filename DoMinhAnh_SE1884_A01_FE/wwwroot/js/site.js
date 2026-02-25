(function () {
    // ── Global Toast Notification ────────────────────────────────────────────
    /**
     * showToast(message, type)
     * type: 'error' | 'warning' | 'success' | 'info'
     * Hiện toast góc trên phải màn hình, tự đóng sau 4 giây.
     */
    window.showToast = function (message, type = 'error') {
        const container = document.getElementById('toastContainer');
        if (!container) return;

        const icons = {
            error:   'bi-x-circle-fill',
            warning: 'bi-exclamation-triangle-fill',
            success: 'bi-check-circle-fill',
            info:    'bi-info-circle-fill'
        };
        const colors = {
            error:   '#dc3545',
            warning: '#fd7e14',
            success: '#198754',
            info:    '#0d6efd'
        };

        const id = 'toast_' + Date.now();
        const color = colors[type] || colors.info;
        const icon  = icons[type]  || icons.info;

        const toastEl = document.createElement('div');
        toastEl.id = id;
        toastEl.className = 'toast align-items-center text-white border-0 shadow';
        toastEl.setAttribute('role', 'alert');
        toastEl.setAttribute('aria-live', 'assertive');
        toastEl.style.cssText = `background-color: ${color}; min-width: 280px; max-width: 380px;`;
        toastEl.innerHTML = `
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center gap-2" style="font-size:0.9rem;">
                    <i class="bi ${icon} flex-shrink-0" style="font-size:1.1rem;"></i>
                    <span>${message}</span>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

        container.appendChild(toastEl);
        const bsToast = new bootstrap.Toast(toastEl, { delay: 4000 });
        bsToast.show();

        // Dọn DOM sau khi ẩn
        toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
    };

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
        showToast('Kết nối mạng đã được khôi phục!', 'success');
    });

    window.addEventListener('offline', () => {
        handleConnectionable();
        showToast('Mất kết nối mạng. Đang dùng dữ liệu cache.', 'warning');
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
