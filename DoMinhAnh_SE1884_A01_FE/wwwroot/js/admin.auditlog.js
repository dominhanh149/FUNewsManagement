// admin.auditlog.js — full AuditLog table (Action / Before / After JSON)

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
    } catch (_) { /* ignore */ }
}

async function loadAuditLog() {
    const entityType  = document.getElementById('filterEntityType').value.trim();
    const action      = document.getElementById('filterAction').value.trim();
    const editorId    = document.getElementById('filterEditor').value;
    const fromDate    = document.getElementById('filterFrom').value;
    const toDate      = document.getElementById('filterTo').value;

    const body = {
        PageNumber: auditPage,
        PageSize:   auditPageSize,
    };
    if (entityType) body.EntityType     = entityType;
    if (action)     body.Action         = action;
    if (editorId)   body.PerformedById  = Number(editorId);
    if (fromDate)   body.FromDate       = fromDate;
    if (toDate)     body.ToDate         = toDate;

    const loadingEl = document.getElementById('loading');
    const tableEl   = document.getElementById('auditTable');
    loadingEl.classList.remove('d-none');
    tableEl.classList.add('d-none');

    try {
        const res    = await api.post('/fe-api/admin/audit-log', body);
        const paging = res?.data ?? res;
        const items  = paging?.items ?? [];
        const total  = paging?.totalCount ?? 0;

        const tbody = document.getElementById('auditBody');
        tbody.innerHTML = items.map(x => {
            const actionBadge = {
                Create: '<span class="badge bg-success">Create</span>',
                Update: '<span class="badge bg-primary">Update</span>',
                Delete: '<span class="badge bg-danger">Delete</span>',
            }[x.action] ?? `<span class="badge bg-secondary">${escHtml(x.action)}</span>`;

            const ts = x.timestamp ? new Date(x.timestamp).toLocaleString('vi-VN') : '—';
            const performer = escHtml(x.performedByName ?? (x.performedById ? `#${x.performedById}` : '—'));

            const before = x.dataBefore
                ? `<button class="btn btn-sm btn-outline-secondary py-0 px-1" onclick='showJson(${JSON.stringify(x.dataBefore)})'>Before</button>`
                : '<span class="text-muted small">—</span>';

            const after = x.dataAfter
                ? `<button class="btn btn-sm btn-outline-primary py-0 px-1" onclick='showJson(${JSON.stringify(x.dataAfter)})'>After</button>`
                : '<span class="text-muted small">—</span>';

            return `
            <tr>
                <td class="px-3 text-muted small font-monospace">${escHtml(x.id?.toString() ?? '')}</td>
                <td>${actionBadge}</td>
                <td class="small">${escHtml(x.entityType ?? '')}</td>
                <td class="small font-monospace text-truncate" style="max-width:140px" title="${escHtml(x.entityId ?? '')}">${escHtml(x.entityId ?? '—')}</td>
                <td>${performer}</td>
                <td class="small text-muted">${ts}</td>
                <td class="text-center">${before}</td>
                <td class="text-center">${after}</td>
            </tr>`;
        }).join('');

        document.getElementById('pagingInfo').textContent =
            `Page ${auditPage} | ${auditPageSize} per page | Total ${total}`;
        document.getElementById('btnPrev').disabled = auditPage <= 1;
        document.getElementById('btnNext').disabled = (auditPage * auditPageSize) >= total;

        tableEl.classList.remove('d-none');
    } catch (e) {
        document.getElementById('auditBody').innerHTML =
            `<tr><td colspan="8" class="text-danger text-center">Error: ${escHtml(e.message)}</td></tr>`;
        tableEl.classList.remove('d-none');
    } finally {
        loadingEl.classList.add('d-none');
    }
}

function showJson(raw) {
    try {
        const pretty = JSON.stringify(JSON.parse(raw), null, 2);
        document.getElementById('jsonModalBody').textContent = pretty;
    } catch {
        document.getElementById('jsonModalBody').textContent = raw;
    }
    new bootstrap.Modal(document.getElementById('jsonModal')).show();
}

function escHtml(s) {
    return String(s ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
}

document.getElementById('btnSearch').addEventListener('click', () => { auditPage = 1; loadAuditLog(); });
document.getElementById('btnReset').addEventListener('click', () => {
    ['filterEntityType','filterAction','filterFrom','filterTo'].forEach(id => document.getElementById(id).value = '');
    document.getElementById('filterEditor').value = '';
    auditPage = 1;
    loadAuditLog();
});
document.getElementById('btnPrev').addEventListener('click', () => { auditPage--; loadAuditLog(); });
document.getElementById('btnNext').addEventListener('click', () => { auditPage++; loadAuditLog(); });

document.addEventListener('DOMContentLoaded', async () => {
    await loadAccounts();
    await loadAuditLog();
});
