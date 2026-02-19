using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Dto.Common;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232__.Repositories.IRepositories;
using Assignmen_PRN232_1.DTOs.Common;
using Assignmen_PRN232_1.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace Assignmen_PRN232_1.Services
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly INewsArticleRepository _newsRepo;
        private readonly ITagRepository _tagRepo;
        private readonly AppDbContext _appDbContext;

        public NewsArticleService(INewsArticleRepository newsRepo, ITagRepository tagRepo, AppDbContext appDbContext)
        {
            _newsRepo = newsRepo;
            _tagRepo = tagRepo;
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<NewsArticleDto>> GetAllAsync()
        {
            var items = await _newsRepo.GetAllAsync();
            return items.Select(MapToDto);
        }

        public async Task<NewsArticleDto?> GetByIdAsync(string id)
        {
            var entity = await _newsRepo.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<PagingDto<NewsArticleDto>> GetListPagingAsync(NewsArticleSearchDto dto)
        {
            var result = await _newsRepo.GetListPagingAsync(dto);
            return new PagingDto<NewsArticleDto>
            {
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Items = result.Items.Select(MapToDto).ToList()
            };
        }

        public async Task<PagingDto<NewsArticleDto>> GetPublicListPagingAsync(NewsArticleSearchDto dto)
        {
            // ép Status=true nếu dto có property Status
            TrySetStatusTrue(dto);

            var result = await _newsRepo.GetListPagingAsync(dto);
            return new PagingDto<NewsArticleDto>
            {
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Items = result.Items.Select(MapToDto).ToList()
            };
        }
        private async Task UpdateNewsTagsAsync(string newsArticleId, List<int> tagIds)
        {
            // XÓA TAGS CŨ
            await _appDbContext.Database.ExecuteSqlRawAsync(
                "DELETE FROM NewsTag WHERE NewsArticleID = {0}",
                newsArticleId);

            // THÊM TAGS MỚI
            if (tagIds != null && tagIds.Any())
            {
                var values = string.Join(",",
                    tagIds.Select(t => $"('{newsArticleId}', {t})"));

                await _appDbContext.Database.ExecuteSqlRawAsync(
                    $"INSERT INTO NewsTag (NewsArticleID, TagID) VALUES {values}");
            }
        }
        public async Task<ApiResponse<NewsArticleDto>> CreateOrEditAsync(NewsArticleSaveDto dto)
        {
            try
            {
                // CREATE
                if (string.IsNullOrWhiteSpace(dto.NewsArticleId))
                {
                    var entity = new NewsArticle
                    {
                        NewsArticleId = GenerateNewsArticleId(),
                        NewsTitle = dto.NewsTitle,
                        Headline = dto.Headline ?? string.Empty,
                        CreatedDate = dto.CreatedDate ?? DateTime.UtcNow,
                        NewsContent = dto.NewsContent,
                        NewsSource = dto.NewsSource,
                        CategoryId = dto.CategoryId,
                        NewsStatus = dto.NewsStatus ?? false,
                        CreatedById = dto.CreatedById
                    };

                    await _newsRepo.AddAsync(entity);
                    await _newsRepo.SaveChangesAsync();

                    // XỬ LÝ TAGS KHI TẠO MỚI
                    if (dto.TagIds != null && dto.TagIds.Any())
                    {
                        await UpdateNewsTagsAsync(entity.NewsArticleId, dto.TagIds);
                    }

                    var created = await _newsRepo.GetByIdAsync(entity.NewsArticleId);
                    return ApiResponse<NewsArticleDto>.SuccessResponse(
                        MapToDto(created ?? entity),
                        "Created successfully.");
                }

                // UPDATE
                var existing = await _newsRepo.GetByIdAsync(dto.NewsArticleId);
                if (existing == null)
                    return ApiResponse<NewsArticleDto>.ErrorResponse("NewsArticle not found.");

                existing.NewsTitle = dto.NewsTitle;
                existing.Headline = dto.Headline ?? existing.Headline;
                existing.NewsContent = dto.NewsContent;
                existing.NewsSource = dto.NewsSource;
                existing.CategoryId = dto.CategoryId;
                existing.NewsStatus = dto.NewsStatus ?? existing.NewsStatus;
                existing.UpdatedById = dto.UpdatedById;
                existing.ModifiedDate = dto.ModifiedDate ?? DateTime.UtcNow;

                await _newsRepo.UpdateAsync(existing);
                await _newsRepo.SaveChangesAsync();

                // XỬ LÝ TAGS KHI CẬP NHẬT - QUAN TRỌNG!
                if (dto.TagIds != null)
                {
                    await UpdateNewsTagsAsync(dto.NewsArticleId, dto.TagIds);
                }

                var updated = await _newsRepo.GetByIdAsync(existing.NewsArticleId);
                return ApiResponse<NewsArticleDto>.SuccessResponse(
                    MapToDto(updated ?? existing),
                    "Updated successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<NewsArticleDto>.ErrorResponse("Operation failed.", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(string id)
        {
            try
            {
                var entity = await _newsRepo.GetByIdAsync(id);
                if (entity == null)
                    return ApiResponse<bool>.ErrorResponse("NewsArticle not found.");

                // delete join table NewsTag: EF many-to-many => clear Tags trước khi delete
                if (entity.Tags != null && entity.Tags.Count > 0)
                {
                    entity.Tags.Clear();
                    await _newsRepo.UpdateAsync(entity);
                    await _newsRepo.SaveChangesAsync();
                }

                await _newsRepo.DeleteAsync(entity);
                await _newsRepo.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse("Delete failed.", ex.Message);
            }
        }

        public async Task<ApiResponse<NewsArticleDto>> DuplicateAsync(string id)
        {
            try
            {
                var entity = await _newsRepo.GetByIdAsync(id);
                if (entity == null)
                    return ApiResponse<NewsArticleDto>.ErrorResponse("NewsArticle not found.");

                var clone = new NewsArticle
                {
                    NewsArticleId = GenerateNewsArticleId(),
                    NewsTitle = entity.NewsTitle,
                    Headline = entity.Headline,
                    CreatedDate = DateTime.UtcNow,
                    NewsContent = entity.NewsContent,
                    NewsSource = entity.NewsSource,
                    CategoryId = entity.CategoryId,
                    NewsStatus = false, // thường duplicate để draft
                    CreatedById = entity.CreatedById
                };

                if (entity.Tags != null && entity.Tags.Count > 0)
                {
                    foreach (var t in entity.Tags)
                        clone.Tags.Add(t);
                }

                await _newsRepo.AddAsync(clone);
                await _newsRepo.SaveChangesAsync();

                var created = await _newsRepo.GetByIdAsync(clone.NewsArticleId);
                return ApiResponse<NewsArticleDto>.SuccessResponse(MapToDto(created ?? clone), "Duplicated successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<NewsArticleDto>.ErrorResponse("Duplicate failed.", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> AddTagAsync(string newsArticleId, int tagId)
        {
            try
            {
                var article = await _newsRepo.GetByIdAsync(newsArticleId);
                if (article == null)
                    return ApiResponse<bool>.ErrorResponse("NewsArticle not found.");

                var tag = await _tagRepo.GetTagByIdAsync(tagId);
                if (tag == null)
                    return ApiResponse<bool>.ErrorResponse("Tag not found.");

                if (article.Tags.Any(t => t.TagId == tagId))
                    return ApiResponse<bool>.SuccessResponse(true, "Tag already attached.");

                article.Tags.Add(tag);

                await _newsRepo.UpdateAsync(article);
                await _newsRepo.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Tag added successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse("Add tag failed.", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> RemoveTagAsync(string newsArticleId, int tagId)
        {
            try
            {
                var article = await _newsRepo.GetByIdAsync(newsArticleId);
                if (article == null)
                    return ApiResponse<bool>.ErrorResponse("NewsArticle not found.");

                var existed = article.Tags.FirstOrDefault(t => t.TagId == tagId);
                if (existed == null)
                    return ApiResponse<bool>.SuccessResponse(true, "Tag not attached.");

                article.Tags.Remove(existed);

                await _newsRepo.UpdateAsync(article);
                await _newsRepo.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Tag removed successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse("Remove tag failed.", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> SetTagsAsync(string newsArticleId, List<int> tagIds)
        {
            try
            {
                var article = await _newsRepo.GetByIdAsync(newsArticleId);
                if (article == null)
                    return ApiResponse<bool>.ErrorResponse("NewsArticle not found.");

                tagIds ??= new List<int>();

                // load tags theo IDs
                var tags = await _tagRepo.GetTagsByIdsAsync(tagIds);

                // reset tag list (join table sẽ sync)
                article.Tags.Clear();
                foreach (var t in tags)
                    article.Tags.Add(t);

                await _newsRepo.UpdateAsync(article);
                await _newsRepo.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Tags updated successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse("Set tags failed.", ex.Message);
            }
        }

        // ===== Helpers =====

        private static string GenerateNewsArticleId() => Guid.NewGuid().ToString("N")[..20];

        private static void TrySetStatusTrue(NewsArticleSearchDto dto)
        {
            var prop = dto.GetType().GetProperty("Status");
            if (prop != null && prop.PropertyType == typeof(bool?))
                prop.SetValue(dto, true);
        }

        private NewsArticleDto MapToDto(NewsArticle x)
        {
            return new NewsArticleDto
            {
                NewsArticleId = x.NewsArticleId,
                NewsTitle = x.NewsTitle,
                Headline = x.Headline,
                CreatedDate = x.CreatedDate,
                NewsContent = x.NewsContent,
                NewsSource = x.NewsSource,
                CategoryId = x.CategoryId,
                CategoryName = x.Category?.CategoryName,
                NewsStatus = x.NewsStatus,
                ViewCount = x.ViewCount,
                CreatedById = x.CreatedById,
                CreatedByName = x.CreatedBy?.AccountName,
                UpdatedById = x.UpdatedById,
                UpdatedByName = x.UpdatedById.HasValue
                    ? (_appDbContext != null
                        ? _appDbContext.SystemAccounts
                            .Where(a => a.AccountId == x.UpdatedById)
                            .Select(a => a.AccountName)
                            .FirstOrDefault()
                        : null)
                    : null,
                ModifiedDate = x.ModifiedDate,
                Tags = x.Tags?.Select(t => new TagDto
                {
                    TagID = t.TagId,
                    TagName = t.TagName,
                    Note = t.Note
                }).ToList() ?? new List<TagDto>()
            };
        }

        public async Task<PagingDto<NewsArticleDto>> GetAuditLogAsync(AuditLogSearchDto dto)
        {
            var query = _appDbContext.NewsArticles
                .Include(x => x.Category)
                .Include(x => x.CreatedBy)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Title))
                query = query.Where(x => x.NewsTitle != null && x.NewsTitle.Contains(dto.Title));

            if (dto.CategoryId.HasValue)
                query = query.Where(x => x.CategoryId == dto.CategoryId);

            if (dto.UpdatedById.HasValue)
                query = query.Where(x => x.UpdatedById == dto.UpdatedById);

            if (dto.FromDate.HasValue)
                query = query.Where(x => x.ModifiedDate >= dto.FromDate);

            if (dto.ToDate.HasValue)
                query = query.Where(x => x.ModifiedDate <= dto.ToDate.Value.AddDays(1));

            if (dto.OnlyModified == true)
                query = query.Where(x => x.UpdatedById != null);

            var total = await query.CountAsync();

            var pageNumber = dto.PageNumber < 1 ? 1 : dto.PageNumber;
            var pageSize   = dto.PageSize   < 1 ? 10 : dto.PageSize;

            var items = await query
                .OrderByDescending(x => x.ModifiedDate ?? x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // get updatedBy names in one query
            var updatedByIds = items
                .Where(x => x.UpdatedById.HasValue)
                .Select(x => x.UpdatedById!.Value)
                .Distinct()
                .ToList();

            var updatedByMap = updatedByIds.Count > 0
                ? await _appDbContext.SystemAccounts
                    .Where(a => updatedByIds.Contains(a.AccountId))
                    .ToDictionaryAsync(a => a.AccountId, a => a.AccountName)
                : new Dictionary<short, string?>();

            var result = items.Select(x => new NewsArticleDto
            {
                NewsArticleId  = x.NewsArticleId,
                NewsTitle      = x.NewsTitle,
                Headline       = x.Headline,
                CategoryId     = x.CategoryId,
                CategoryName   = x.Category?.CategoryName,
                NewsStatus     = x.NewsStatus,
                CreatedDate    = x.CreatedDate,
                CreatedById    = x.CreatedById,
                CreatedByName  = x.CreatedBy?.AccountName,
                UpdatedById    = x.UpdatedById,
                UpdatedByName  = x.UpdatedById.HasValue && updatedByMap.TryGetValue(x.UpdatedById.Value, out var uname)
                                     ? uname : null,
                ModifiedDate   = x.ModifiedDate,
                ViewCount      = x.ViewCount
            }).ToList();

            return new PagingDto<NewsArticleDto>
            {
                PageNumber = pageNumber,
                PageSize   = pageSize,
                TotalCount = total,
                Items      = result
            };
        }

        public async Task<bool> IncreaseViewAsync(string id)
        {
            var article = await _appDbContext.NewsArticles.FindAsync(id);

            if (article == null)
                return false;

            article.ViewCount++;

            await _appDbContext.SaveChangesAsync();

            return true;
        }

    }
}
