using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232__.Repositories.IRepositories;
using Assignmen_PRN232_1.Data;
using Assignmen_PRN232_1.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace Assignmen_PRN232__.Repositories
{
    public class NewsArticleRepository
    : BaseRepository<NewsArticle, AppDbContext>, INewsArticleRepository
    {
        public NewsArticleRepository(AppDbContext context, IUnitOfWork unitOfWork)
            : base(context, unitOfWork)
        {
        }

        public async Task<PagingDto<NewsArticle>> GetListPagingAsync(NewsArticleSearchDto searchDto)
        {
            var query = _dbContext.Set<NewsArticle>()
                .Include(x => x.Category)
                .Include(x => x.Tags)
                .Include(x => x.CreatedBy)
                .AsNoTracking();


            if (!string.IsNullOrWhiteSpace(searchDto.Title))
            {
                query = query.Where(x => x.NewsTitle.Contains(searchDto.Title));
            }


            if (!string.IsNullOrWhiteSpace(searchDto.Author))
            {
                query = query.Where(x => x.CreatedBy != null && x.CreatedBy.AccountName.Contains(searchDto.Author));
            }


            if (!string.IsNullOrWhiteSpace(searchDto.SortBy))
            {
                var keyword = searchDto.SortBy.Trim();
                query = query.Where(x =>
                    x.NewsTitle.Contains(keyword) ||
                    x.Headline.Contains(keyword) ||
                    (x.NewsContent != null && x.NewsContent.Contains(keyword)));
            }


            if (searchDto.CategoryId.HasValue && searchDto.CategoryId > 0)
            {
                query = query.Where(x => x.CategoryId == searchDto.CategoryId);
            }


            if (searchDto.Status.HasValue)
            {
                query = query.Where(x => x.NewsStatus == searchDto.Status);
            }


            if (searchDto.FromDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= searchDto.FromDate);
            }
            if (searchDto.ToDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate <= searchDto.ToDate);
            }


            if (searchDto.CreatedById.HasValue && searchDto.CreatedById > 0)
            {
                query = query.Where(x => x.CreatedById == searchDto.CreatedById);
            }

            var totalRecords = await query.CountAsync();


            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return new PagingDto<NewsArticle>
            {
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize,
                TotalCount = totalRecords,
                Items = items
            };
        }

        public async Task<bool> ExistsByIdAsync(string newsArticleId)
        {
            return await ExistsAsync(x => x.NewsArticleId == newsArticleId);
        }


        public async Task<NewsArticle?> GetByIdAsync(string id)
        {
            return await _dbContext.Set<NewsArticle>()
                .Include(x => x.Tags)
                .Include(x => x.Category)
                .Include(x => x.CreatedBy)
                .FirstOrDefaultAsync(x => x.NewsArticleId == id);
        }
    }
}
