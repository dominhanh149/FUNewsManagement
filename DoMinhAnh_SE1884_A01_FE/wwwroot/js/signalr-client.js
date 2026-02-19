// SignalR Client

const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7053/newsHub")
    .withAutomaticReconnect()
    .build();

// Start connection
console.log("Loading SignalR Client...");

async function startSignalR() {
    try {
        await hubConnection.start();
        console.log("SignalR Connected.");
        showToast("SignalR: Connected", "success");
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        showToast("SignalR: Connection Failed (See Console)", "error");
        setTimeout(startSignalR, 5000);
    }
}

// Global event handlers
hubConnection.on("ReceiveArticleUpdate", function (notification) {
    console.log("ReceiveArticleUpdate:", notification);
    
    // Show toast for any create/update/delete
    const msg = `Article '${notification.newsTitle || notification.newsArticleId}' was ${notification.action}.`;
    showToast(msg, "info");

    // If we are on the public news page, refresh the list if needed
    if (typeof loadNews === 'function') {
        loadNews();
    }
    // If we are on reports/audit page, maybe refresh? 
    // For now, let's just refresh public news list.
});

hubConnection.on("ReceiveViewUpdate", function (notification) {
    // 1. Update list item if exists
    const elList = document.getElementById(`view-count-${notification.newsArticleId}`);
    if (elList) {
        updateViewElement(elList, notification.viewCount);
    }

    // 2. Update detail page if matches
    if (window.__articleId && window.__articleId === notification.newsArticleId) {
        const elDetail = document.getElementById('articleViews'); // defined in Detail.cshtml
        if (elDetail) {
            updateViewElement(elDetail, notification.viewCount);
        }
    }
});

function updateViewElement(el, count) {
    el.textContent = count.toLocaleString('vi-VN');
    el.classList.add('text-success', 'fw-bold');
    setTimeout(() => el.classList.remove('text-success', 'fw-bold'), 1000);
}

// Toast helper (reusing existing toast container in _Layout if available)
function showToast(message, type = 'info') {
    const container = document.getElementById('toastContainer');
    if (!container) return; // or create one

    const bg = type === 'error' ? 'bg-danger' : (type === 'success' ? 'bg-success' : 'bg-info');
    const toastHtml = `
        <div class="toast align-items-center text-white ${bg} border-0 show" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>`;
    
    // Create element from HTML string
    const temp = document.createElement('div');
    temp.innerHTML = toastHtml.trim();
    const toastEl = temp.firstChild;
    
    container.appendChild(toastEl);
    
    // Auto remove
    setTimeout(() => {
        toastEl.remove();
    }, 5000);
}

// Start
document.addEventListener("DOMContentLoaded", startSignalR);
