// SignalR Client — v5

const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7053/newsHub")
    .withAutomaticReconnect()
    .build();

console.log("Loading SignalR Client...");

// ── Notification History (max 10) ────────────────────────────────────────────
const NOTIF_KEY = 'funews_notifications';
const MAX_NOTIF = 10;

function getNotifications() {
    try { return JSON.parse(localStorage.getItem(NOTIF_KEY)) ?? []; } catch { return []; }
}

function saveNotifications(list) {
    try { localStorage.setItem(NOTIF_KEY, JSON.stringify(list.slice(0, MAX_NOTIF))); } catch {}
}

/**
 * Thêm notification mới vào history và cập nhật UI
 * @param {string} title - Tiêu đề/tên bài
 * @param {string} action - 'created' | 'updated' | 'deleted'
 * @param {string} articleId
 */
function pushNotification(title, action, articleId) {
    const list = getNotifications();
    list.unshift({
        id: Date.now(),
        title,
        action,
        articleId,
        time: new Date().toISOString()
    });
    saveNotifications(list);
    renderNotifDropdown(list);
    updateNotifBadge(list.length);
}

function renderNotifDropdown(list) {
    const ul = document.getElementById('notifList');
    const emptyEl = document.getElementById('notifEmpty');
    if (!ul) return;

    if (!list || list.length === 0) {
        emptyEl?.classList.remove('d-none');
        // Xóa các item cũ (ngoài emptyEl)
        [...ul.children].forEach(c => { if (c.id !== 'notifEmpty') c.remove(); });
        return;
    }

    emptyEl?.classList.add('d-none');

    // Re-render items
    const items = list.slice(0, MAX_NOTIF);
    const existing = [...ul.querySelectorAll('.notif-item')];
    existing.forEach(el => el.remove());

    const actionIcon = { created: '🆕', updated: '✏️', deleted: '🗑️' };
    const actionColor = { created: '#198754', updated: '#0d6efd', deleted: '#dc3545' };

    items.forEach(n => {
        const li = document.createElement('li');
        li.className = 'notif-item px-3 py-2 border-bottom';
        li.style.cssText = 'cursor:default; font-size:.85rem; transition: background .15s;';
        li.onmouseenter = () => li.style.background = '#f8f9fa';
        li.onmouseleave = () => li.style.background = '';

        const timeAgo = formatTimeAgo(new Date(n.time));
        const icon = actionIcon[n.action] ?? '📌';
        const color = actionColor[n.action] ?? '#6c757d';

        li.innerHTML = `
            <div class="d-flex align-items-start gap-2">
                <span style="font-size:1.1rem">${icon}</span>
                <div class="flex-grow-1">
                    <div style="font-weight:600; color:${color};">
                        ${n.action === 'created' ? 'Bài viết mới' : n.action === 'updated' ? 'Đã cập nhật' : 'Đã xóa'}
                    </div>
                    <div class="text-truncate" style="max-width:240px;" title="${n.title}">${n.title}</div>
                    <div class="text-muted" style="font-size:.75rem;">${timeAgo}</div>
                </div>
            </div>`;
        ul.appendChild(li);
    });
}

function updateNotifBadge(count) {
    const badge = document.getElementById('notifBadge');
    if (!badge) return;
    if (count > 0) {
        badge.textContent = count > 99 ? '99+' : count;
        badge.classList.remove('d-none');
    } else {
        badge.classList.add('d-none');
    }
}

function formatTimeAgo(date) {
    const diff = Math.floor((Date.now() - date.getTime()) / 1000);
    if (diff < 60)  return `${diff}s trước`;
    if (diff < 3600) return `${Math.floor(diff / 60)}ph trước`;
    if (diff < 86400) return `${Math.floor(diff / 3600)}h trước`;
    return date.toLocaleDateString('vi-VN');
}

// Xóa tất cả notifications
window.clearNotifications = function () {
    saveNotifications([]);
    renderNotifDropdown([]);
    updateNotifBadge(0);
};

// Khi mở dropdown → reset badge về 0 (đã xem)
document.addEventListener('DOMContentLoaded', () => {
    const bell = document.getElementById('notifBell');
    if (bell) {
        bell.addEventListener('click', () => {
            // Badge reset khi mở xem
            setTimeout(() => updateNotifBadge(0), 300);
        });
    }

    // Khởi tạo: render từ localStorage ngay khi load trang
    const saved = getNotifications();
    renderNotifDropdown(saved);
    // Không hiện badge cho notif cũ (đã xem từ session trước)
});

// ── SignalR Retry ─────────────────────────────────────────────────────────────
let _signalRRetryCount = 0;
const _signalRMaxRetry = 3;

async function startSignalR() {
    try {
        await hubConnection.start();
        console.log("SignalR Connected.");
        _signalRRetryCount = 0;
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        _signalRRetryCount++;
        if (_signalRRetryCount === 1) {
            if (typeof window.showToast === 'function')
                window.showToast("SignalR: Connection Failed", "error");
        }
        if (_signalRRetryCount <= _signalRMaxRetry) {
            setTimeout(startSignalR, 5000 * _signalRRetryCount);
        } else {
            console.warn("SignalR: Stopped retrying after", _signalRMaxRetry, "attempts.");
        }
    }
}

// ── Event Handlers ────────────────────────────────────────────────────────────
hubConnection.on("ReceiveArticleUpdate", function (notification) {
    console.log("ReceiveArticleUpdate:", notification);

    const title = notification.newsTitle || notification.newsArticleId || 'Unknown';
    const action = (notification.action ?? '').toLowerCase();

    // ✅ Chỉ thông báo khi có bài viết MỚI được tạo
    if (action === 'created') {
        pushNotification(title, action, notification.newsArticleId);
        if (typeof window.showToast === 'function')
            window.showToast(`🆕 Bài viết mới: "${title}"`, 'info');
    }

    // Reload danh sách cho mọi loại action
    if (typeof loadNews === 'function') {
        loadNews();
    }
});


hubConnection.on("ReceiveViewUpdate", function (notification) {
    const elList = document.getElementById(`view-count-${notification.newsArticleId}`);
    if (elList) updateViewElement(elList, notification.viewCount);

    if (window.__articleId && window.__articleId === notification.newsArticleId) {
        const elDetail = document.getElementById('articleViews');
        if (elDetail) updateViewElement(elDetail, notification.viewCount);
    }
});

function updateViewElement(el, count) {
    el.textContent = count.toLocaleString('vi-VN');
    el.classList.add('text-success', 'fw-bold');
    setTimeout(() => el.classList.remove('text-success', 'fw-bold'), 1000);
}

// Start
document.addEventListener("DOMContentLoaded", startSignalR);
