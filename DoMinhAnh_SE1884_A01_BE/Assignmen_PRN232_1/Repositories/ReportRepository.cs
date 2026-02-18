using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232__.Repositories.IRepositories;
using Assignmen_PRN232_1.Data;
using Microsoft.EntityFrameworkCore;

namespace Assignmen_PRN232__.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NewsArticleReportDto> GetNewsArticleReportAsync(ReportSearchDto searchDto)
        {
            var query = _context.NewsArticles
                .AsNoTracking()
                .AsQueryable();

            // Filter by date range
            if (searchDto.FromDate.HasValue)
                query = query.Where(x => x.CreatedDate >= searchDto.FromDate.Value);

            if (searchDto.ToDate.HasValue)
                query = query.Where(x => x.CreatedDate <= searchDto.ToDate.Value);

            // Filter by category/author
            if (searchDto.CategoryId.HasValue)
                query = query.Where(x => x.CategoryId == searchDto.CategoryId.Value);

            if (searchDto.AuthorId.HasValue)
                query = query.Where(x => x.CreatedById == searchDto.AuthorId.Value);

            // Total counts
            var totalArticles = await query.CountAsync();
            var totalActive = await query.CountAsync(x => x.NewsStatus == true);
            var totalInactive = totalArticles - totalActive;

            // Category stats (group by CategoryId + CategoryName)
            var categoryStats = await query
                .GroupBy(x => new { x.CategoryId, CategoryName = x.Category.CategoryName })
                .Select(g => new CategoryStatDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    ArticleCount = g.Count(),
                    ActiveCount = g.Count(x => x.NewsStatus == true),
                    InactiveCount = g.Count(x => x.NewsStatus == false)
                })
                .OrderByDescending(x => x.ArticleCount)
                .ToListAsync();

            // Author stats (group by CreatedById + AuthorName)
            var authorStats = await query
                .GroupBy(x => new { x.CreatedById, AuthorName = x.CreatedBy.AccountName })
                .Select(g => new AuthorStatDto
                {
                    AuthorId = g.Key.CreatedById,
                    AuthorName = g.Key.AuthorName,
                    ArticleCount = g.Count(),
                    ActiveCount = g.Count(x => x.NewsStatus == true),
                    InactiveCount = g.Count(x => x.NewsStatus == false)
                })
                .OrderByDescending(x => x.ArticleCount)
                .ToListAsync();

            return new NewsArticleReportDto
            {
                TotalArticles = totalArticles,
                TotalActive = totalActive,
                TotalInactive = totalInactive,
                CategoryStats = categoryStats,
                AuthorStats = authorStats
            };
        }
    }
}
