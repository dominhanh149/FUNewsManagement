using AnalyticsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly FunewsManagementContext _db;

    public ReportsController(FunewsManagementContext db)
    {
        _db = db;
    }

    /// <summary>
    /// POST /api/reports/news-articles
    /// Endpoint được FE gọi qua proxy để lấy thống kê bài viết với filter.
    /// </summary>
    [HttpPost("news-articles")]
    public async Task<IActionResult> NewsArticles([FromBody] ReportFilterDto filter)
    {
        filter ??= new ReportFilterDto();

        var q = _db.NewsArticles
            .Include(x => x.Category)
            .Include(x => x.CreatedBy)
            .AsQueryable();

        // Apply filters
        if (filter.FromDate.HasValue)
            q = q.Where(x => x.CreatedDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            q = q.Where(x => x.CreatedDate <= filter.ToDate.Value);

        if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
            q = q.Where(x => x.CategoryId == filter.CategoryId.Value);

        if (filter.AuthorId.HasValue && filter.AuthorId.Value > 0)
            q = q.Where(x => x.CreatedById == filter.AuthorId.Value);

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

        return Ok(new NewsArticleReportDto
        {
            TotalArticles = total,
            TotalActive = active,
            TotalInactive = total - active,
            CategoryStats = byCategory,
            AuthorStats = byAuthor
        });
    }
}
