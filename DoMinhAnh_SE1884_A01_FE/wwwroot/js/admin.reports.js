document.getElementById("btnRun").addEventListener("click", async () => {
    const fromDate = document.getElementById("fromDate").value;
    const toDate = document.getElementById("toDate").value;
    const rawCategoryId = document.getElementById("categoryId").value;
    const rawAuthorId = document.getElementById("authorId").value;

    const categoryId =
        rawCategoryId !== "" && Number(rawCategoryId) >= 0
            ? Number(rawCategoryId)
            : null;

    const authorId =
        rawAuthorId !== "" && Number(rawAuthorId) >= 0
            ? Number(rawAuthorId)
            : null;

    const body = {
        fromDate: fromDate ? new Date(fromDate).toISOString() : null,
        toDate: toDate ? new Date(toDate).toISOString() : null,
        categoryId: categoryId ? Number(categoryId) : null,
        authorId: authorId ? Number(authorId) : null,
        pageNumber: 1,
        pageSize: 10
    };

    try {
        const r = await api.post("/fe-api/Reports/news-articles", body);

        document.getElementById("summary").innerHTML = `
      <div class="alert alert-info">
        <b>Total:</b> ${r.totalArticles} |
        <b>Active:</b> ${r.totalActive} |
        <b>Inactive:</b> ${r.totalInactive}
      </div>
    `;

        const tbC = document.getElementById("tbCategory");
        tbC.innerHTML = (r.categoryStats || []).map(x => `
      <tr>
        <td>${x.categoryName ?? ("#" + x.categoryId)}</td>
        <td>${x.articleCount}</td>
        <td>${x.activeCount}</td>
        <td>${x.inactiveCount}</td>
      </tr>
    `).join("");

        const tbA = document.getElementById("tbAuthor");
        tbA.innerHTML = (r.authorStats || []).map(x => `
      <tr>
        <td>${x.authorName ?? ("#" + x.authorId)}</td>
        <td>${x.articleCount}</td>
        <td>${x.activeCount}</td>
        <td>${x.inactiveCount}</td>
      </tr>
    `).join("");

    } catch (e) {
        alert("Report error: " + (e.message || e));
    }
});
