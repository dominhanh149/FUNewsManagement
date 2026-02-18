
using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232__.Repositories.IRepositories;
using Assignmen_PRN232_1.Data;
using Assignmen_PRN232_1.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace Assignmen_PRN232__.Repositories
{
    /// <summary>
    /// Repository implementation for Tag entity operations
    /// Inherits from BaseRepository for common CRUD operations
    /// </summary>
    public class TagRepository : BaseRepository<Tag, AppDbContext>, ITagRepository
    {
        public TagRepository(AppDbContext dbContext, IUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        {
        }

        #region Basic CRUD Operations

        public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
        {
            var query = @"
                SELECT 
                    t.TagID,
                    t.TagName,
                    t.Note,
                    COUNT(nt.NewsArticleID) as ArticleCount
                FROM Tag t
                LEFT JOIN NewsTag nt ON t.TagID = nt.TagID
                GROUP BY t.TagID, t.TagName, t.Note
                ORDER BY t.TagName";

            return await DapperQueryAsync<TagDto>(query);
        }

        public async Task<Tag?> GetTagByIdAsync(int tagId)
        {
            return await GetByIdAsync(tagId);
        }

        public async Task<TagWithArticlesDto?> GetTagWithArticlesAsync(int tagId)
        {
            var tag = await FindByCondition(t => t.TagId == tagId)
                .FirstOrDefaultAsync();

            if (tag == null)
                return null;

            var articles = await GetArticlesByTagAsync(tagId);

            return new TagWithArticlesDto
            {
                TagID = tag.TagId,
                TagName = tag.TagName ?? string.Empty,
                Note = tag.Note,
                Articles = articles.ToList()
            };
        }

        public async Task<Tag> CreateTagAsync(CreateTagDto createTagDto)
        {
            // 1) Check TagName duplicate
            if (await TagNameExistsAsync(createTagDto.TagName))
                throw new InvalidOperationException($"Tag name '{createTagDto.TagName}' already exists.");

            // ✅ 2) Generate new TagID
            var newId = await GenerateTagIdAsync();

            var tag = new Tag
            {
                TagId = newId,
                TagName = createTagDto.TagName,
                Note = createTagDto.Note
            };

            await AddAsync(tag);
            await SaveChangesAsync();

            return tag;
        }

        private async Task<int> GenerateTagIdAsync()
        {
            int max = await _dbContext.Tags
                .Select(t => (int?)t.TagId)
                .MaxAsync() ?? 0;

            return max + 1;
        }


        public async Task<bool> UpdateTagAsync(UpdateTagDto updateTagDto)
        {
            var existingTag = await GetTagByIdAsync(updateTagDto.TagID);
            if (existingTag == null)
                return false;

            // Check if new tag name already exists (excluding current tag)
            if (await TagNameExistsAsync(updateTagDto.TagName, updateTagDto.TagID))
            {
                throw new InvalidOperationException($"Tag name '{updateTagDto.TagName}' already exists.");
            }

            existingTag.TagName = updateTagDto.TagName;
            existingTag.Note = updateTagDto.Note;

            await UpdateAsync(existingTag);
            await SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTagAsync(int tagId)
        {
            // Check if tag is used in any articles
            if (await IsTagUsedInArticlesAsync(tagId))
            {
                throw new InvalidOperationException(
                    "Cannot delete tag because it is being used in one or more articles. " +
                    "Please remove the tag from all articles before deleting.");
            }

            var tag = await GetTagByIdAsync(tagId);
            if (tag == null)
                return false;

            await DeleteAsync(tag);
            await SaveChangesAsync();

            return true;
        }

        #endregion

        #region Validation and Check Methods

        public async Task<bool> TagExistsAsync(int tagId)
        {
            return await ExistsAsync(t => t.TagId == tagId);
        }

        public async Task<bool> TagNameExistsAsync(string tagName, int? excludeTagId = null)
        {
            var query = FindByCondition(t => t.TagName!.ToLower() == tagName.ToLower());

            if (excludeTagId.HasValue)
            {
                query = query.Where(t => t.TagId != excludeTagId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsTagUsedInArticlesAsync(int tagId)
        {
            var sql = "SELECT COUNT(*) FROM NewsTag WHERE TagID = @tagId";
            var count = await DapperExecuteScalarAsync<int>(sql, new { tagId });
            return count > 0;
        }

        public async Task<int> GetArticleCountByTagAsync(int tagId)
        {
            var sql = "SELECT COUNT(*) FROM NewsTag WHERE TagID = @tagId";
            return await DapperExecuteScalarAsync<int>(sql, new { tagId });
        }

        #endregion

        #region Search and Filter Methods

        public async Task<(IEnumerable<TagDto> Tags, int TotalCount)> SearchTagsAsync(TagSearchDto searchDto)
        {
            var sqlCount = @"
                SELECT COUNT(DISTINCT t.TagID)
                FROM Tag t
                LEFT JOIN NewsTag nt ON t.TagID = nt.TagID
                WHERE 1=1";

            var sqlQuery = @"
                SELECT 
                    t.TagID,
                    t.TagName,
                    t.Note,
                    COUNT(nt.NewsArticleID) as ArticleCount
                FROM Tag t
                LEFT JOIN NewsTag nt ON t.TagID = nt.TagID
                WHERE 1=1";

            var parameters = new Dictionary<string, object>();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchDto.TagName))
            {
                sqlCount += " AND t.TagName LIKE @tagName";
                sqlQuery += " AND t.TagName LIKE @tagName";
                parameters.Add("tagName", $"%{searchDto.TagName}%");
            }

            // Group by for query
            sqlQuery += " GROUP BY t.TagID, t.TagName, t.Note";

            // Apply HasArticles filter after GROUP BY
            if (searchDto.HasArticles.HasValue)
            {
                if (searchDto.HasArticles.Value)
                {
                    sqlQuery += " HAVING COUNT(nt.NewsArticleID) > 0";
                }
                else
                {
                    sqlQuery += " HAVING COUNT(nt.NewsArticleID) = 0";
                }
            }

            // Apply sorting
            var sortColumn = string.IsNullOrWhiteSpace(searchDto.SortBy) ? "TagName" : searchDto.SortBy;
            var sortDirection = searchDto.SortDirection?.ToLower() == "desc" ? "DESC" : "ASC";
            sqlQuery += $" ORDER BY {sortColumn} {sortDirection}";

            // Apply pagination
            sqlQuery += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";
            parameters.Add("skip", searchDto.Skip);
            parameters.Add("pageSize", searchDto.PageSize);

            // Execute queries
            var totalCount = await DapperExecuteScalarAsync<int>(sqlCount, parameters);
            var tags = await DapperQueryAsync<TagDto>(sqlQuery, parameters);

            return (tags, totalCount);
        }

        public async Task<IEnumerable<TagArticleDto>> GetArticlesByTagAsync(int tagId)
        {
            var sql = @"
                SELECT 
                    na.NewsArticleID,
                    na.NewsTitle,
                    na.Headline,
                    na.CreatedDate,
                    na.NewsStatus,
                    c.CategoryName,
                    sa.AccountName as CreatedByName
                FROM NewsTag nt
                INNER JOIN NewsArticle na ON nt.NewsArticleID = na.NewsArticleID
                LEFT JOIN Category c ON na.CategoryID = c.CategoryID
                LEFT JOIN SystemAccount sa ON na.CreatedByID = sa.AccountID
                WHERE nt.TagID = @tagId
                ORDER BY na.CreatedDate DESC";

            return await DapperQueryAsync<TagArticleDto>(sql, new { tagId });
        }

        public async Task<IEnumerable<TagDto>> GetTagsByArticleIdAsync(string articleId)
        {
            var sql = @"
                SELECT 
                    t.TagID,
                    t.TagName,
                    t.Note,
                    0 as ArticleCount
                FROM NewsTag nt
                INNER JOIN Tag t ON nt.TagID = t.TagID
                WHERE nt.NewsArticleID = @articleId
                ORDER BY t.TagName";

            return await DapperQueryAsync<TagDto>(sql, new { articleId });
        }

        #endregion

        #region Bulk Operations

        public async Task<IEnumerable<Tag>> GetTagsByIdsAsync(List<int> tagIds)
        {
            return await FindByCondition(t => tagIds.Contains(t.TagId))
                .ToListAsync();
        }

        public async Task<IEnumerable<TagDto>> GetUnusedTagsAsync()
        {
            var sql = @"
                SELECT 
                    t.TagID,
                    t.TagName,
                    t.Note,
                    0 as ArticleCount
                FROM Tag t
                LEFT JOIN NewsTag nt ON t.TagID = nt.TagID
                WHERE nt.TagID IS NULL
                ORDER BY t.TagName";

            return await DapperQueryAsync<TagDto>(sql);
        }

        public async Task<IEnumerable<TagDto>> GetMostUsedTagsAsync(int topCount = 10)
        {
            var sql = @"
                SELECT TOP (@topCount)
                    t.TagID,
                    t.TagName,
                    t.Note,
                    COUNT(nt.NewsArticleID) as ArticleCount
                FROM Tag t
                INNER JOIN NewsTag nt ON t.TagID = nt.TagID
                GROUP BY t.TagID, t.TagName, t.Note
                ORDER BY ArticleCount DESC, t.TagName";

            return await DapperQueryAsync<TagDto>(sql, new { topCount });
        }

        #endregion
    }
}