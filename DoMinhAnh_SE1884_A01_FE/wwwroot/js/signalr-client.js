// SignalR Client

const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7053/newsHub")
    .withAutomaticReconnect()
    .build();

console.log("Loading SignalR Client...");

// Giới hạn retry để tránh spam toast lỗi
let _signalRRetryCount = 0;
const _signalRMaxRetry = 3;

async function startSignalR() {
    try {
        await hubConnection.start();
        console.log("SignalR Connected.");
        _signalRRetryCount = 0; // reset khi thành công
        // KHÔNG hiện toast "Connected" ở đây để tránh spam mỗi lần chuyển trang
    } catch (err) {
        console.error("SignalR Connection Error: ", err);

        _signalRRetryCount++;
        if (_signalRRetryCount === 1) {
            // Chỉ hiện toast 1 lần duy nhất (lần đầu fail)
            if (typeof window.showToast === 'function')
                window.showToast("SignalR: Connection Failed (See Console)", "error");
        }

        // Retry tối đa 3 lần với backoff (5s, 10s, 15s)
        if (_signalRRetryCount <= _signalRMaxRetry) {
            setTimeout(startSignalR, 5000 * _signalRRetryCount);
        } else {
            console.warn("SignalR: Stopped retrying after", _signalRMaxRetry, "attempts.");
        }
    }
}

// Global event handlers
hubConnection.on("ReceiveArticleUpdate", function (notification) {
    console.log("ReceiveArticleUpdate:", notification);
    
    const msg = `Article '${notification.newsTitle || notification.newsArticleId}' was ${notification.action}.`;
    if (typeof window.showToast === 'function')
        window.showToast(msg, "info");

    if (typeof loadNews === 'function') {
        loadNews();
    }
});

hubConnection.on("ReceiveViewUpdate", function (notification) {
    const elList = document.getElementById(`view-count-${notification.newsArticleId}`);
    if (elList) {
        updateViewElement(elList, notification.viewCount);
    }

    if (window.__articleId && window.__articleId === notification.newsArticleId) {
        const elDetail = document.getElementById('articleViews');
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

// Start
document.addEventListener("DOMContentLoaded", startSignalR);
