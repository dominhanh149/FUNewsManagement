using AnalyticsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/recommend")]
public class RecommendController : ControllerBase
{
    private readonly FunewsManagementContext _db;

    public RecommendController(FunewsManagementContext db)
    {
        _db = db;
    }

    /// <summary>
    /// GET /api/recommend/{id}
    /// Gợi ý 3 bài viết liên quan dựa trên tags và category.
    /// Fallback: nếu không đủ 3 bài theo tag, bổ sung bài cùng category.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Recommend(string id)
    {
        var article = await _db.NewsArticles
            .Include(x => x.Tags)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.NewsArticleId == id);

        if (article == null)
            return NotFound(new { message = $"Article '{id}' not found." });

        var tagIds = article.Tags.Select(x => x.TagId).ToList();

        // Bước 1: Tìm bài có cùng tag
        var byTag = await _db.NewsArticles
            .Include(x => x.Category)
            .Where(x => x.NewsArticleId != id && x.NewsStatus == true)
            .Where(x => x.Tags.Any(t => tagIds.Contains(t.TagId)))
            .OrderByDescending(x => x.Tags.Count(t => tagIds.Contains(t.TagId)))
            .ThenByDescending(x => x.ViewCount)
            .Take(3)
            .Select(x => new RecommendArticleDto
            {
                NewsArticleId = x.NewsArticleId,
                NewsTitle = x.NewsTitle,
                Headline = x.Headline,
                CategoryName = x.Category != null ? x.Category.CategoryName : null,
                CreatedDate = x.CreatedDate,
                ViewCount = x.ViewCount
            })
            .ToListAsync();

        // Bước 2: Nếu chưa đủ 3, bổ sung bài cùng category
        if (byTag.Count < 3 && article.CategoryId.HasValue)
        {
            var existingIds = byTag.Select(x => x.NewsArticleId).ToList();
            existingIds.Add(id);

            var needed = 3 - byTag.Count;
            var byCategory = await _db.NewsArticles
                .Include(x => x.Category)
                .Where(x => !existingIds.Contains(x.NewsArticleId)
                         && x.NewsStatus == true
                         && x.CategoryId == article.CategoryId)
                .OrderByDescending(x => x.ViewCount)
                .Take(needed)
                .Select(x => new RecommendArticleDto
                {
                    NewsArticleId = x.NewsArticleId,
                    NewsTitle = x.NewsTitle,
                    Headline = x.Headline,
                    CategoryName = x.Category != null ? x.Category.CategoryName : null,
                    CreatedDate = x.CreatedDate,
                    ViewCount = x.ViewCount
                })
                .ToListAsync();

            byTag.AddRange(byCategory);
        }

        return Ok(byTag);
    }
}
