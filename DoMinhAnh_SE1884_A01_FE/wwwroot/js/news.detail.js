/**
 * news.detail.js – Trang chi tiết bài viết
 * - Tự động tăng ViewCount khi mở trang
 * - Load bài viết từ Core API
 * - Load related articles từ Analytics API (recommend)
 */

(function () {
    const articleId = window.__articleId;

    // ─── Toast ────────────────────────────────────────────────────────────────
    function showToast(msg, type = 'success') {
        const container = document.getElementById('toastContainer');
        if (!container) return;
        const id = 'toast_' + Date.now();
        const bg = type === 'success' ? '#11998e' : '#f5576c';
        const icon = type === 'success' ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill';
        container.insertAdjacentHTML('beforeend', `
            <div id="${id}" class="toast align-items-center text-white border-0 show"
                 style="background:${bg};border-radius:12px;min-width:260px" role="alert">
                <div class="d-flex">
                    <div class="toast-body d-flex align-items-center gap-2">
                        <i class="bi ${icon}"></i> ${msg}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto"></button>
                </div>
            </div>`);
        setTimeout(() => document.getElementById(id)?.remove(), 4000);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    function safe(v) { return v ?? ''; }

    function unwrap(res) {
        // Hỗ trợ ApiResponse { data: ... } hoặc trả thẳng object
        return res?.data ?? res;
    }

    function formatDate(raw) {
        if (!raw) return '—';
        return new Date(raw).toLocaleDateString('vi-VN', {
            day: '2-digit', month: '2-digit', year: 'numeric'
        });
    }

    // ─── Render article ───────────────────────────────────────────────────────
    function renderArticle(n) {
        const isActive = n.newsStatus === true;

        // Breadcrumb
        document.getElementById('bcCategory').textContent = n.categoryName || 'Chưa phân loại';
        const title = n.newsTitle || '(Không có tiêu đề)';
        document.getElementById('bcTitle').textContent =
            title.length > 40 ? title.slice(0, 40) + '…' : title;

        // Category badge
        const catEl = document.getElementById('articleCategory');
        catEl.textContent = n.categoryName || 'Chưa phân loại';

        // Status badge
        const statusEl = document.getElementById('articleStatus');
        statusEl.textContent = isActive ? 'Active' : 'Inactive';
        statusEl.style.background = isActive ? '#d4edda' : '#f8d7da';
        statusEl.style.color = isActive ? '#155724' : '#721c24';

        // Title & headline
        document.getElementById('articleTitle').textContent = title;
        const headlineEl = document.getElementById('articleHeadline');
        if (n.headline) {
            headlineEl.textContent = n.headline;
        } else {
            headlineEl.classList.add('d-none');
        }

        // Meta
        document.getElementById('articleAuthor').textContent =
            n.createdByName || n.authorName || 'Ẩn danh';
        document.getElementById('articleDate').textContent = formatDate(n.createdDate);
        document.getElementById('articleViews').textContent =
            (n.viewCount ?? 0).toLocaleString('vi-VN');

        // Source link
        if (n.newsSource) {
            const srcEl = document.getElementById('articleSource');
            srcEl.classList.remove('d-none');
            const link = document.getElementById('articleSourceLink');
            link.href = n.newsSource.startsWith('http') ? n.newsSource : '#';
            link.textContent = n.newsSource;
        }

        // Tags
        const tagsEl = document.getElementById('articleTags');
        const tags = n.tags ?? n.Tags ?? [];
        if (tags.length) {
            tagsEl.innerHTML = tags.map(t => {
                const name = t.tagName ?? t.TagName ?? t.name ?? '';
                return `<span class="badge rounded-pill px-3 py-2"
                              style="background:#f0f0ff;color:#6c63ff;font-size:.8rem;border:1px solid #d8d4ff">
                            <i class="bi bi-tag-fill me-1" style="font-size:.7rem"></i>${name}
                        </span>`;
            }).join('');
        } else {
            tagsEl.classList.add('d-none');
        }

        // Content (render as HTML nếu có thẻ, ngược lại wrap trong <p>)
        const body = n.newsContent ?? n.NewsContent ?? '';
        const bodyEl = document.getElementById('articleBody');
        if (body.trim().startsWith('<')) {
            bodyEl.innerHTML = body;
        } else {
            // 1. Simple Markdown Image Parser: ![alt](url) → <img ... />
            let html = body.replace(/!\[(.*?)\]\((.*?)\)/g, (match, alt, url) => {
                return `<img src="${url}" alt="${alt}" class="img-fluid rounded my-3 d-block" />`;
            });

            // 2. Plain text → chia đoạn theo newline
            bodyEl.innerHTML = html
                .split(/\n{2,}/)
                .map(p => `<p>${p.replace(/\n/g, '<br>')}</p>`)
                .join('');
        }

        // Update page title
        document.title = `${title} – FUNews`;
    }

    // ─── Render related articles ──────────────────────────────────────────────
    function renderRelated(list) {
        document.getElementById('relatedLoading').classList.add('d-none');

        if (!list || list.length === 0) {
            document.getElementById('relatedEmpty').classList.remove('d-none');
            return;
        }

        const container = document.getElementById('relatedList');
        container.innerHTML = list.map(r => {
            const id = r.newsArticleId ?? r.NewsArticleId ?? '';
            const title = r.newsTitle ?? r.NewsTitle ?? '(Không có tiêu đề)';
            const category = r.categoryName ?? r.CategoryName ?? '';
            const views = r.viewCount ?? r.ViewCount ?? 0;
            const date = formatDate(r.createdDate ?? r.CreatedDate);

            return `
            <a href="/News/Detail/${id}" class="related-card">
                <div class="d-flex align-items-start gap-2">
                    <div class="rounded-circle d-flex align-items-center justify-content-center flex-shrink-0 text-white"
                         style="width:36px;height:36px;background:linear-gradient(135deg,#6c63ff,#4facfe);font-size:.7rem;font-weight:700">
                        ${title.charAt(0).toUpperCase()}
                    </div>
                    <div style="min-width:0">
                        <div class="fw-semibold" style="font-size:.88rem;line-height:1.3;
                             white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                            ${title}
                        </div>
                        <div class="d-flex align-items-center gap-2 mt-1 flex-wrap" style="font-size:.75rem;color:#999">
                            ${category ? `<span class="badge rounded-pill" style="background:#ede9ff;color:#6c63ff">${category}</span>` : ''}
                            <span><i class="bi bi-eye-fill me-1" style="color:#f5576c"></i>${views.toLocaleString()}</span>
                            <span>${date}</span>
                        </div>
                    </div>
                </div>
            </a>`;
        }).join('');
    }

    // ─── Tăng ViewCount ───────────────────────────────────────────────────────
    async function increaseView(id) {
        try {
            await fetch(`/fe-api/staff/news/${id}/view`, {
                method: 'POST',
                credentials: 'include'
            });
        } catch (e) {
            // Silent fail – không ảnh hưởng UX
            console.warn('increaseView failed:', e.message);
        }
    }

    // ─── Load bài viết chính ──────────────────────────────────────────────────
    async function loadArticle(id) {
        try {
            const res = await api.get(`/fe-api/staff/news/${id}`);
            const article = unwrap(res);

            if (!article || (!article.newsArticleId && !article.NewsArticleId)) {
                throw new Error('Article not found');
            }

            renderArticle(article);

            document.getElementById('loadingSkeleton').classList.add('d-none');
            document.getElementById('articleContent').classList.remove('d-none');

        } catch (e) {
            document.getElementById('loadingSkeleton').classList.add('d-none');
            document.getElementById('errorState').classList.remove('d-none');
        }
    }

    // ─── Load related articles ────────────────────────────────────────────────
    async function loadRelated(id) {
        try {
            const res = await api.get(`/fe-api/staff/news/recommend/${id}`);
            const list = Array.isArray(res) ? res : (res?.data ?? []);
            renderRelated(list);
        } catch (e) {
            document.getElementById('relatedLoading').classList.add('d-none');
            document.getElementById('relatedEmpty').classList.remove('d-none');
        }
    }

    // ─── Init ─────────────────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', async () => {
        if (!articleId) {
            document.getElementById('loadingSkeleton').classList.add('d-none');
            document.getElementById('errorState').classList.remove('d-none');
            return;
        }

        // Chạy song song: load bài viết + tăng view + load related
        await Promise.all([
            loadArticle(articleId),
            increaseView(articleId),
            loadRelated(articleId)
        ]);
    });
})();
