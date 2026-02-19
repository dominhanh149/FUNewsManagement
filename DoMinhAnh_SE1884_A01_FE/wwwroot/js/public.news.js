let pageNumber = 1;
const pageSize = 10;

async function loadNews() {
    try {
        const keyword = document.getElementById("keyword").value.trim();

        // ✅ giống Accounts: body paging (vì BE public đang là POST /public/paging)
        const body = {
            pageNumber,
            pageSize,
            title: keyword || null
        };

        const data = await api.post("/fe-api/staff/news/public/paging", body)

        console.log("loadNews data:", data);

        // ✅ giống Accounts: unwrap linh hoạt
        let paging;
        if (data?.data?.items) {
            paging = data.data;                 // ApiResponse { data: PagingDto }
        } else if (data?.items) {
            paging = data;                      // PagingDto trực tiếp
        } else if (data?.data?.Items) {
            // PascalCase fallback
            paging = {
                items: data.data.Items,
                totalCount: data.data.TotalCount,
                pageNumber: data.data.PageNumber,
                pageSize: data.data.PageSize
            };
        } else if (data?.Items) {
            paging = {
                items: data.Items,
                totalCount: data.TotalCount,
                pageNumber: data.PageNumber,
                pageSize: data.PageSize
            };
        } else {
            console.error("Invalid response format:", data);
            paging = { items: [], totalCount: 0, pageNumber: 1, pageSize };
        }

        const items = paging.items || [];
        const total = paging.totalCount ?? items.length;

        // Render table
        const tb = document.getElementById("tbNews");
        tb.innerHTML = items.map(x => {
            const id = x.newsArticleId ?? x.NewsArticleId ?? "";
            const title = x.newsTitle ?? x.NewsTitle ?? "";
            const category = x.categoryName ?? x.CategoryName ?? "";
            const createdRaw = x.createdDate ?? x.CreatedDate ?? null;
            const created = createdRaw ? new Date(createdRaw).toLocaleDateString("vi-VN") : "";
            const statusVal = (x.newsStatus ?? x.NewsStatus) === true;
            const views = (x.viewCount ?? x.ViewCount ?? 0).toLocaleString("vi-VN");

            return `
                <tr>
                    <td class="fw-semibold">${title}</td>
                    <td>${category}</td>
                    <td>${created}</td>
                    <td>
                        <span class="badge ${statusVal ? "bg-success" : "bg-secondary"}">
                            ${statusVal ? "Active" : "Inactive"}
                        </span>
                    </td>
                    <td class="text-center">
                        <span style="color:#f5576c;font-weight:600">
                            <i class="bi bi-eye-fill me-1"></i>${views}
                        </span>
                    </td>
                    <td class="text-center">
                        <a href="/News/Detail/${id}"
                           class="btn btn-sm text-white"
                           style="background:linear-gradient(135deg,#6c63ff,#4facfe);border-radius:8px;border:none;font-size:.8rem">
                            <i class="bi bi-arrow-right-circle me-1"></i>Xem chi tiết
                        </a>
                    </td>
                </tr>
            `;
        }).join("");

        // ✅ SỬA ID: view là newsPagingInfo / btnNewsPrev / btnNewsNext
        document.getElementById("newsPagingInfo").innerText =
            `Page ${paging.pageNumber ?? pageNumber} | Size ${paging.pageSize ?? pageSize} | Total ${total}`;

        document.getElementById("btnNewsPrev").disabled = pageNumber <= 1;
        document.getElementById("btnNewsNext").disabled = (pageNumber * pageSize) >= total;

    } catch (e) {
        console.error("loadNews error:", e);
        alert("Failed to load news: " + (e.message || e));
    }
}

// Events
document.getElementById("btnSearch").addEventListener("click", async () => {
    pageNumber = 1;
    await loadNews();
});

document.getElementById("btnNewsPrev").addEventListener("click", async () => {
    if (pageNumber > 1) pageNumber--;
    await loadNews();
});

document.getElementById("btnNewsNext").addEventListener("click", async () => {
    pageNumber++;
    await loadNews();
});

document.addEventListener("DOMContentLoaded", loadNews);
