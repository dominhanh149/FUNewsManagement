// admin.auditlog.js

let auditPage = 1;
const auditPageSize = 15;

async function loadAccounts() {
    try {
        const res = await api.get('/fe-api/admin/accounts');
        const items = res?.data?.items ?? res?.items ?? (Array.isArray(res) ? res : []);
        const sel = document.getElementById('filterEditor');
        items.forEach(a => {
            const opt = document.createElement('option');
            opt.value = a.accountId;
            opt.textContent = `${a.accountName ?? ''} (${a.accountEmail ?? ''})`;
            sel.appendChild(opt);
        });
    } catch (_) { /* nếu lỗi: bỏ qua */ }
}

async function loadAuditLog() {
    const title      = document.getElementById('filterTitle').value.trim();
    const editorId   = document.getElementById('filterEditor').value;
    const fromDate   = document.getElementById('filterFrom').value;
    const toDate     = document.getElementById('filterTo').value;
    const onlyMod    = document.getElementById('filterOnlyMod').checked;

    const body = {
        PageNumber: auditPage,
        PageSize: auditPageSize,
    };
    if (title)    body.Title       = title;
    if (editorId) body.UpdatedById = Number(editorId);
    if (fromDate) body.FromDate    = fromDate;
    if (toDate)   body.ToDate      = toDate;
    if (onlyMod)  body.OnlyModified = true;

    const loadingEl = document.getElementById('loading');
    const tableEl   = document.getElementById('auditTable');
    loadingEl.classList.remove('d-none');
    tableEl.classList.add('d-none');

    try {
        const res = await api.post('/fe-api/admin/audit-log', body);
        const paging = res?.data ?? res;
        const items  = paging?.items ?? [];
        const total  = paging?.totalCount ?? 0;

        const tbody = document.getElementById('auditBody');
        tbody.innerHTML = items.map(x => {
            const status = x.newsStatus
                ? '<span class="badge bg-success">Active</span>'
                : '<span class="badge bg-secondary">Inactive</span>';
            const createdDate  = x.createdDate   ? new Date(x.createdDate).toLocaleString('vi-VN')   : '—';
            const modifiedDate = x.modifiedDate  ? new Date(x.modifiedDate).toLocaleString('vi-VN')  : '—';
            const editor       = x.updatedByName || (x.updatedById ? `#${x.updatedById}` : '<em class="text-muted">Not edited</em>');
            return `
            <tr>
                <td class="text-muted small font-monospace">${x.newsArticleId ?? ''}</td>
                <td>
                    <div class="fw-semibold">${escHtml(x.newsTitle ?? x.headline ?? '')}</div>
                    <small class="text-muted">${escHtml(x.categoryName ?? '—')}</small>
                </td>
                <td>${status}</td>
                <td>
                    <div>${escHtml(x.createdByName ?? '—')}</div>
                    <small class="text-muted">${createdDate}</small>
                </td>
                <td>
                    <div class="fw-semibold text-primary">${editor}</div>
                    <small class="text-muted">${modifiedDate}</small>
                </td>
                <td class="text-center">${x.viewCount ?? 0}</td>
            </tr>`;
        }).join('');

        // paging info
        document.getElementById('pagingInfo').textContent =
            `Page ${auditPage} | ${auditPageSize} per page | Total ${total}`;
        document.getElementById('btnPrev').disabled = auditPage <= 1;
        document.getElementById('btnNext').disabled = (auditPage * auditPageSize) >= total;

        tableEl.classList.remove('d-none');
    } catch (e) {
        document.getElementById('auditBody').innerHTML =
            `<tr><td colspan="6" class="text-danger text-center">Error: ${escHtml(e.message)}</td></tr>`;
        tableEl.classList.remove('d-none');
    } finally {
        loadingEl.classList.add('d-none');
    }
}

function escHtml(s) {
    return String(s ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
}

document.getElementById('btnSearch').addEventListener('click', () => {
    auditPage = 1;
    loadAuditLog();
});
document.getElementById('btnReset').addEventListener('click', () => {
    document.getElementById('filterTitle').value   = '';
    document.getElementById('filterEditor').value  = '';
    document.getElementById('filterFrom').value    = '';
    document.getElementById('filterTo').value      = '';
    document.getElementById('filterOnlyMod').checked = false;
    auditPage = 1;
    loadAuditLog();
});
document.getElementById('btnPrev').addEventListener('click', () => { auditPage--; loadAuditLog(); });
document.getElementById('btnNext').addEventListener('click', () => { auditPage++; loadAuditLog(); });

document.addEventListener('DOMContentLoaded', async () => {
    await loadAccounts();
    await loadAuditLog();
});
