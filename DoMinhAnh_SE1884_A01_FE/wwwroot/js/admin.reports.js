/**
 * admin.reports.js – Dashboard Admin
 * Gọi qua FE proxy: /fe-api/Reports/dashboard, /fe-api/Reports/trending, /fe-api/Reports/export
 */

(function () {
    // ─── Chart instances ────────────────────────────────────────────────────
    let chartCategory = null;
    let chartAuthor = null;

    // ─── Palette ────────────────────────────────────────────────────────────
    const PALETTE = [
        '#6c63ff', '#4facfe', '#11998e', '#38ef7d',
        '#f093fb', '#f5576c', '#fda085', '#f6d365',
        '#a18cd1', '#fbc2eb', '#84fab0', '#8fd3f4'
    ];

    // ─── Toast helper ────────────────────────────────────────────────────────
    function showToast(msg, type = 'success') {
        const container = document.getElementById('toastContainer');
        const id = 'toast_' + Date.now();
        const icon = type === 'success' ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill';
        const bg = type === 'success' ? '#11998e' : '#f5576c';
        container.insertAdjacentHTML('beforeend', `
            <div id="${id}" class="toast align-items-center text-white border-0 show"
                 style="background:${bg};border-radius:12px;min-width:280px" role="alert">
                <div class="d-flex">
                    <div class="toast-body d-flex align-items-center gap-2">
                        <i class="bi ${icon}"></i> ${msg}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto"
                            data-bs-dismiss="toast"></button>
                </div>
            </div>`);
        setTimeout(() => document.getElementById(id)?.remove(), 4000);
    }

    // ─── Loading ─────────────────────────────────────────────────────────────
    function setLoading(on) {
        document.getElementById('loadingSpinner').classList.toggle('d-none', !on);
        document.getElementById('btnRun').disabled = on;
        document.getElementById('btnExport').disabled = on;
    }

    // ─── Show / hide sections ────────────────────────────────────────────────
    function showSections(show) {
        ['summaryCards', 'chartsSection', 'tablesSection', 'trendingSection']
            .forEach(id => document.getElementById(id).classList.toggle('d-none', !show));
    }

    // ─── Build query string from filter ─────────────────────────────────────
    function buildQuery() {
        const params = new URLSearchParams();
        const from = document.getElementById('fromDate').value;
        const to = document.getElementById('toDate').value;
        const cat = document.getElementById('categoryId').value;
        const auth = document.getElementById('authorId').value;
        if (from) params.append('fromDate', new Date(from).toISOString());
        if (to) params.append('toDate', new Date(to).toISOString());
        if (cat && Number(cat) > 0) params.append('categoryId', cat);
        if (auth && Number(auth) > 0) params.append('authorId', auth);
        return params.toString();
    }

    // ─── Render summary cards ────────────────────────────────────────────────
    function renderSummary(data) {
        document.getElementById('statTotal').textContent = data.totalArticles ?? 0;
        document.getElementById('statActive').textContent = data.totalActive ?? 0;
        document.getElementById('statInactive').textContent = data.totalInactive ?? 0;
    }

    // ─── Render Pie chart (by category) ─────────────────────────────────────
    function renderCategoryChart(stats) {
        const labels = stats.map(x => x.categoryName || `#${x.categoryId}`);
        const values = stats.map(x => x.articleCount);
        const ctx = document.getElementById('chartCategory').getContext('2d');

        if (chartCategory) chartCategory.destroy();
        chartCategory = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels,
                datasets: [{
                    data: values,
                    backgroundColor: PALETTE.slice(0, labels.length),
                    borderWidth: 2,
                    borderColor: '#fff',
                    hoverOffset: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { padding: 16, font: { size: 12 }, usePointStyle: true }
                    },
                    tooltip: {
                        callbacks: {
                            label: ctx => ` ${ctx.label}: ${ctx.parsed} bài`
                        }
                    }
                },
                cutout: '60%'
            }
        });
    }

    // ─── Render Bar chart (by author) ────────────────────────────────────────
    function renderAuthorChart(stats) {
        const top = stats.slice(0, 8);
        const labels = top.map(x => x.authorName || `#${x.authorId}`);
        const active = top.map(x => x.activeCount);
        const inactive = top.map(x => x.inactiveCount);
        const ctx = document.getElementById('chartAuthor').getContext('2d');

        if (chartAuthor) chartAuthor.destroy();
        chartAuthor = new Chart(ctx, {
            type: 'bar',
            data: {
                labels,
                datasets: [
                    {
                        label: 'Active',
                        data: active,
                        backgroundColor: 'rgba(56, 239, 125, 0.8)',
                        borderColor: '#11998e',
                        borderWidth: 1.5,
                        borderRadius: 6
                    },
                    {
                        label: 'Inactive',
                        data: inactive,
                        backgroundColor: 'rgba(245, 87, 108, 0.8)',
                        borderColor: '#f5576c',
                        borderWidth: 1.5,
                        borderRadius: 6
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        labels: { font: { size: 12 }, usePointStyle: true }
                    }
                },
                scales: {
                    x: {
                        stacked: false,
                        grid: { display: false },
                        ticks: { font: { size: 11 } }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { stepSize: 1, font: { size: 11 } },
                        grid: { color: 'rgba(0,0,0,.05)' }
                    }
                }
            }
        });
    }

    // ─── Render category table ────────────────────────────────────────────────
    function renderCategoryTable(stats) {
        const tb = document.getElementById('tbCategory');
        if (!stats.length) {
            tb.innerHTML = '<tr><td colspan="4" class="text-center text-muted py-3">Không có dữ liệu</td></tr>';
            return;
        }
        tb.innerHTML = stats.map((x, i) => `
            <tr>
                <td>
                    <span class="d-inline-block rounded-circle me-2"
                          style="width:10px;height:10px;background:${PALETTE[i % PALETTE.length]}"></span>
                    <strong>${x.categoryName || `#${x.categoryId}`}</strong>
                </td>
                <td class="text-center fw-semibold">${x.articleCount}</td>
                <td class="text-center">
                    <span class="badge" style="background:#d4edda;color:#155724">${x.activeCount}</span>
                </td>
                <td class="text-center">
                    <span class="badge" style="background:#f8d7da;color:#721c24">${x.inactiveCount}</span>
                </td>
            </tr>`).join('');
    }

    // ─── Render author table ──────────────────────────────────────────────────
    function renderAuthorTable(stats) {
        const tb = document.getElementById('tbAuthor');
        if (!stats.length) {
            tb.innerHTML = '<tr><td colspan="4" class="text-center text-muted py-3">Không có dữ liệu</td></tr>';
            return;
        }
        tb.innerHTML = stats.map(x => `
            <tr>
                <td>
                    <div class="d-flex align-items-center gap-2">
                        <div class="rounded-circle d-flex align-items-center justify-content-center text-white fw-bold"
                             style="width:32px;height:32px;background:linear-gradient(135deg,#6c63ff,#4facfe);font-size:.75rem;flex-shrink:0">
                            ${(x.authorName || '?').charAt(0).toUpperCase()}
                        </div>
                        <span>${x.authorName || `#${x.authorId}`}</span>
                    </div>
                </td>
                <td class="text-center fw-semibold">${x.articleCount}</td>
                <td class="text-center">
                    <span class="badge" style="background:#d4edda;color:#155724">${x.activeCount}</span>
                </td>
                <td class="text-center">
                    <span class="badge" style="background:#f8d7da;color:#721c24">${x.inactiveCount}</span>
                </td>
            </tr>`).join('');
    }

    // ─── Render trending table ────────────────────────────────────────────────
    function renderTrending(list) {
        const tb = document.getElementById('tbTrending');
        if (!list.length) {
            tb.innerHTML = '<tr><td colspan="5" class="text-center text-muted py-3">Không có dữ liệu</td></tr>';
            return;
        }
        tb.innerHTML = list.map((x, i) => {
            const medal = i === 0 ? '🥇' : i === 1 ? '🥈' : i === 2 ? '🥉' : `${i + 1}`;
            const date = x.createdDate ? new Date(x.createdDate).toLocaleDateString('vi-VN') : '—';
            return `
            <tr>
                <td class="text-center fw-bold" style="width:50px">${medal}</td>
                <td>
                    <div class="fw-semibold" style="max-width:320px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                        ${x.newsTitle || '(Không có tiêu đề)'}
                    </div>
                    ${x.headline ? `<small class="text-muted" style="font-size:.78rem;display:block;max-width:320px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">${x.headline}</small>` : ''}
                </td>
                <td>
                    ${x.categoryName
                        ? `<span class="badge rounded-pill" style="background:#ede9ff;color:#6c63ff">${x.categoryName}</span>`
                        : '<span class="text-muted">—</span>'}
                </td>
                <td class="text-center">
                    <span class="fw-bold" style="color:#f5576c">
                        <i class="bi bi-eye-fill me-1"></i>${(x.viewCount ?? 0).toLocaleString()}
                    </span>
                </td>
                <td class="text-muted" style="font-size:.85rem">${date}</td>
            </tr>`;
        }).join('');
    }

    // ─── Load dashboard data ──────────────────────────────────────────────────
    async function loadDashboard() {
        setLoading(true);
        showSections(false);

        try {
            const qs = buildQuery();
            const url = '/fe-api/Reports/dashboard' + (qs ? '?' + qs : '');
            const data = await api.get(url);

            renderSummary(data);
            renderCategoryChart(data.categoryStats || []);
            renderAuthorChart(data.authorStats || []);
            renderCategoryTable(data.categoryStats || []);
            renderAuthorTable(data.authorStats || []);

            showSections(true);
            showToast('Dữ liệu đã được tải thành công!');
        } catch (e) {
            showToast('Lỗi tải dashboard: ' + (e.message || e), 'error');
        } finally {
            setLoading(false);
        }
    }

    // ─── Load trending data ───────────────────────────────────────────────────
    async function loadTrending() {
        try {
            const data = await api.get('/fe-api/Reports/trending');
            const list = Array.isArray(data) ? data : (data.data || []);
            renderTrending(list);
            document.getElementById('trendingSection').classList.remove('d-none');
        } catch (e) {
            console.warn('Trending load failed:', e.message);
        }
    }

    // ─── Export Excel ─────────────────────────────────────────────────────────
    async function exportExcel() {
        const btn = document.getElementById('btnExport');
        const originalHtml = btn.innerHTML;
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Đang xuất...';

        try {
            const qs = buildQuery();
            const url = '/fe-api/Reports/export' + (qs ? '?' + qs : '');

            const res = await fetch(url, { credentials: 'include' });
            if (!res.ok) throw new Error('Export thất bại (HTTP ' + res.status + ')');

            const blob = await res.blob();
            const a = document.createElement('a');
            a.href = URL.createObjectURL(blob);
            a.download = `FUNews_Report_${new Date().toISOString().slice(0, 10)}.xlsx`;
            a.click();
            URL.revokeObjectURL(a.href);
            showToast('Xuất Excel thành công!');
        } catch (e) {
            showToast('Lỗi xuất Excel: ' + (e.message || e), 'error');
        } finally {
            btn.disabled = false;
            btn.innerHTML = originalHtml;
        }
    }

    // ─── Init ─────────────────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', () => {
        document.getElementById('btnRun').addEventListener('click', loadDashboard);
        document.getElementById('btnRefresh').addEventListener('click', () => {
            loadDashboard();
            loadTrending();
        });
        document.getElementById('btnExport').addEventListener('click', exportExcel);

        // Auto-load on page open
        loadDashboard();
        loadTrending();
    });
})();
