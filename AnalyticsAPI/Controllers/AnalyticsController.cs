using AnalyticsAPI.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly FunewsManagementContext _db;
    private readonly IMemoryCache _cache;

    public AnalyticsController(FunewsManagementContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    /// <summary>
    /// GET /api/analytics/dashboard
    /// Thống kê bài viết theo category và author, hỗ trợ filter.
    /// Kết quả được cache 5 phút nếu không có filter.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] short? categoryId,
        [FromQuery] short? authorId)
    {
        bool hasFilter = fromDate.HasValue || toDate.HasValue || categoryId.HasValue || authorId.HasValue;
        string cacheKey = "dashboard_all";

        if (!hasFilter && _cache.TryGetValue(cacheKey, out NewsArticleReportDto? cached))
            return Ok(cached);

        var q = _db.NewsArticles
            .Include(x => x.Category)
            .Include(x => x.CreatedBy)
            .AsQueryable();

        if (fromDate.HasValue)
            q = q.Where(x => x.CreatedDate >= fromDate.Value);
        if (toDate.HasValue)
            q = q.Where(x => x.CreatedDate <= toDate.Value);
        if (categoryId.HasValue && categoryId.Value > 0)
            q = q.Where(x => x.CategoryId == categoryId.Value);
        if (authorId.HasValue && authorId.Value > 0)
            q = q.Where(x => x.CreatedById == authorId.Value);

        var total = await q.CountAsync();
        var active = await q.CountAsync(x => x.NewsStatus == true);

        var byCategory = await q
            .GroupBy(x => new { x.CategoryId, x.Category!.CategoryName })
            .Select(g => new CategoryStatDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                ArticleCount = g.Count(),
                ActiveCount = g.Count(x => x.NewsStatus == true),
                InactiveCount = g.Count(x => x.NewsStatus != true)
            })
            .ToListAsync();

        var byAuthor = await q
            .GroupBy(x => new { x.CreatedById, x.CreatedBy!.AccountName })
            .Select(g => new AuthorStatDto
            {
                AuthorId = g.Key.CreatedById,
                AuthorName = g.Key.AccountName,
                ArticleCount = g.Count(),
                ActiveCount = g.Count(x => x.NewsStatus == true),
                InactiveCount = g.Count(x => x.NewsStatus != true)
            })
            .ToListAsync();

        var result = new NewsArticleReportDto
        {
            TotalArticles = total,
            TotalActive = active,
            TotalInactive = total - active,
            CategoryStats = byCategory,
            AuthorStats = byAuthor
        };

        if (!hasFilter)
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return Ok(result);
    }

    /// <summary>
    /// GET /api/analytics/trending
    /// Danh sách bài viết trending (nhiều view nhất), hỗ trợ filter date.
    /// </summary>
    [HttpGet("trending")]
    public async Task<IActionResult> Trending(
        [FromQuery] int top = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var q = _db.NewsArticles
            .Include(x => x.Category)
            .Where(x => x.NewsStatus == true)
            .AsQueryable();

        if (fromDate.HasValue)
            q = q.Where(x => x.CreatedDate >= fromDate.Value);
        if (toDate.HasValue)
            q = q.Where(x => x.CreatedDate <= toDate.Value);

        var result = await q
            .OrderByDescending(x => x.ViewCount)
            .ThenByDescending(x => x.CreatedDate)
            .Take(top)
            .Select(x => new TrendingArticleDto
            {
                NewsArticleId = x.NewsArticleId,
                NewsTitle = x.NewsTitle,
                Headline = x.Headline,
                ViewCount = x.ViewCount,
                CategoryName = x.Category != null ? x.Category.CategoryName : null,
                CreatedDate = x.CreatedDate,
                NewsStatus = x.NewsStatus
            })
            .ToListAsync();

        return Ok(result);
    }

    /// <summary>
    /// GET /api/analytics/export
    /// Export báo cáo Excel 3 sheets: Summary, By Category, By Author.
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var q = _db.NewsArticles
            .Include(x => x.Category)
            .Include(x => x.CreatedBy)
            .AsQueryable();

        if (fromDate.HasValue)
            q = q.Where(x => x.CreatedDate >= fromDate.Value);
        if (toDate.HasValue)
            q = q.Where(x => x.CreatedDate <= toDate.Value);

        var total = await q.CountAsync();
        var active = await q.CountAsync(x => x.NewsStatus == true);

        var byCategory = await q
            .GroupBy(x => new { x.CategoryId, x.Category!.CategoryName })
            .Select(g => new
            {
                g.Key.CategoryName,
                Total = g.Count(),
                Active = g.Count(x => x.NewsStatus == true),
                Inactive = g.Count(x => x.NewsStatus != true)
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        var byAuthor = await q
            .GroupBy(x => new { x.CreatedById, x.CreatedBy!.AccountName })
            .Select(g => new
            {
                g.Key.AccountName,
                Total = g.Count(),
                Active = g.Count(x => x.NewsStatus == true),
                Inactive = g.Count(x => x.NewsStatus != true)
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        using var wb = new XLWorkbook();

        // Sheet 1: Summary
        var wsSummary = wb.Worksheets.Add("Summary");
        wsSummary.Cell(1, 1).Value = "Metric";
        wsSummary.Cell(1, 2).Value = "Value";
        wsSummary.Row(1).Style.Font.Bold = true;
        wsSummary.Cell(2, 1).Value = "Total Articles";
        wsSummary.Cell(2, 2).Value = total;
        wsSummary.Cell(3, 1).Value = "Active Articles";
        wsSummary.Cell(3, 2).Value = active;
        wsSummary.Cell(4, 1).Value = "Inactive Articles";
        wsSummary.Cell(4, 2).Value = total - active;
        if (fromDate.HasValue) { wsSummary.Cell(5, 1).Value = "From Date"; wsSummary.Cell(5, 2).Value = fromDate.Value.ToString("yyyy-MM-dd"); }
        if (toDate.HasValue) { wsSummary.Cell(6, 1).Value = "To Date"; wsSummary.Cell(6, 2).Value = toDate.Value.ToString("yyyy-MM-dd"); }
        wsSummary.Columns().AdjustToContents();

        // Sheet 2: By Category
        var wsCategory = wb.Worksheets.Add("By Category");
        wsCategory.Cell(1, 1).Value = "Category";
        wsCategory.Cell(1, 2).Value = "Total";
        wsCategory.Cell(1, 3).Value = "Active";
        wsCategory.Cell(1, 4).Value = "Inactive";
        wsCategory.Row(1).Style.Font.Bold = true;
        for (int i = 0; i < byCategory.Count; i++)
        {
            wsCategory.Cell(i + 2, 1).Value = byCategory[i].CategoryName ?? "(No Category)";
            wsCategory.Cell(i + 2, 2).Value = byCategory[i].Total;
            wsCategory.Cell(i + 2, 3).Value = byCategory[i].Active;
            wsCategory.Cell(i + 2, 4).Value = byCategory[i].Inactive;
        }
        wsCategory.Columns().AdjustToContents();

        // Sheet 3: By Author
        var wsAuthor = wb.Worksheets.Add("By Author");
        wsAuthor.Cell(1, 1).Value = "Author";
        wsAuthor.Cell(1, 2).Value = "Total";
        wsAuthor.Cell(1, 3).Value = "Active";
        wsAuthor.Cell(1, 4).Value = "Inactive";
        wsAuthor.Row(1).Style.Font.Bold = true;
        for (int i = 0; i < byAuthor.Count; i++)
        {
            wsAuthor.Cell(i + 2, 1).Value = byAuthor[i].AccountName ?? "(Unknown)";
            wsAuthor.Cell(i + 2, 2).Value = byAuthor[i].Total;
            wsAuthor.Cell(i + 2, 3).Value = byAuthor[i].Active;
            wsAuthor.Cell(i + 2, 4).Value = byAuthor[i].Inactive;
        }
        wsAuthor.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);

        var fileName = $"FUNews_Report_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
