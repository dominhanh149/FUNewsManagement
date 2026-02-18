using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Dto.Common;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232__.Repositories.IRepositories;
using Assignmen_PRN232_1.DTOs.Common;
using Assignmen_PRN232_1.Services.IServices;
using Mapster;

namespace Assignmen_PRN232_1.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        }

        #region Basic CRUD Operations

        public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAllTagsAsync();
        }

        public async Task<TagDto?> GetTagByIdAsync(int tagId)
        {
            var tag = await _tagRepository.GetTagByIdAsync(tagId);
            if (tag == null)
                return null;

            var articleCount = await _tagRepository.GetArticleCountByTagAsync(tagId);

            return new TagDto
            {
                TagID = tag.TagId,
                TagName = tag.TagName ?? string.Empty,
                Note = tag.Note,
                ArticleCount = articleCount
            };
        }

        public async Task<TagWithArticlesDto?> GetTagWithArticlesAsync(int tagId)
        {
            return await _tagRepository.GetTagWithArticlesAsync(tagId);
        }

        public async Task<TagDto> CreateTagAsync(CreateTagDto createTagDto)
        {
            // Validate tag name availability
            if (!await IsTagNameAvailableAsync(createTagDto.TagName))
            {
                throw new InvalidOperationException($"Tag name '{createTagDto.TagName}' already exists.");
            }

            // Create tag
            var tag = await _tagRepository.CreateTagAsync(createTagDto);

            return new TagDto
            {
                TagID = tag.TagId,
                TagName = tag.TagName ?? string.Empty,
                Note = tag.Note,
                ArticleCount = 0
            };
        }

        public async Task<bool> UpdateTagAsync(UpdateTagDto updateTagDto)
        {
            // Check if tag exists
            if (!await _tagRepository.TagExistsAsync(updateTagDto.TagID))
            {
                throw new KeyNotFoundException($"Tag with ID {updateTagDto.TagID} not found.");
            }

            // Validate tag name availability (excluding current tag)
            if (!await IsTagNameAvailableAsync(updateTagDto.TagName, updateTagDto.TagID))
            {
                throw new InvalidOperationException($"Tag name '{updateTagDto.TagName}' already exists.");
            }

            return await _tagRepository.UpdateTagAsync(updateTagDto);
        }

        public async Task<bool> DeleteTagAsync(int tagId)
        {
            // Check if tag can be deleted
            var (canDelete, reason) = await CanDeleteTagAsync(tagId);
            if (!canDelete)
            {
                throw new InvalidOperationException(reason ?? "Cannot delete tag.");
            }

            return await _tagRepository.DeleteTagAsync(tagId);
        }

        #endregion

        #region Search and Filter

        public async Task<PaginatedTagResponse> SearchTagsAsync(TagSearchDto searchDto)
        {
            var (tags, totalCount) = await _tagRepository.SearchTagsAsync(searchDto);

            return new PaginatedTagResponse
            {
                Tags = tags,
                TotalCount = totalCount,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize
            };
        }

        public async Task<IEnumerable<TagArticleDto>> GetArticlesByTagAsync(int tagId)
        {
            // Verify tag exists
            if (!await _tagRepository.TagExistsAsync(tagId))
            {
                throw new KeyNotFoundException($"Tag with ID {tagId} not found.");
            }

            return await _tagRepository.GetArticlesByTagAsync(tagId);
        }

        public async Task<IEnumerable<TagDto>> GetTagsByArticleIdAsync(string articleId)
        {
            if (string.IsNullOrWhiteSpace(articleId))
            {
                throw new ArgumentException("Article ID cannot be empty.", nameof(articleId));
            }

            return await _tagRepository.GetTagsByArticleIdAsync(articleId);
        }

        #endregion

        #region Validation

        public async Task<bool> TagExistsAsync(int tagId)
        {
            return await _tagRepository.TagExistsAsync(tagId);
        }

        public async Task<bool> IsTagNameAvailableAsync(string tagName, int? excludeTagId = null)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                throw new ArgumentException("Tag name cannot be empty.", nameof(tagName));
            }

            var exists = await _tagRepository.TagNameExistsAsync(tagName, excludeTagId);
            return !exists;
        }

        public async Task<(bool CanDelete, string? Reason)> CanDeleteTagAsync(int tagId)
        {
            // Check if tag exists
            if (!await _tagRepository.TagExistsAsync(tagId))
            {
                return (false, $"Tag with ID {tagId} not found.");
            }

            // Check if tag is used in any articles
            if (await _tagRepository.IsTagUsedInArticlesAsync(tagId))
            {
                var articleCount = await _tagRepository.GetArticleCountByTagAsync(tagId);
                return (false, $"Cannot delete tag because it is being used in {articleCount} article(s). " +
                              "Please remove the tag from all articles before deleting.");
            }

            return (true, null);
        }

        #endregion

        #region Statistics

        public async Task<IEnumerable<TagDto>> GetUnusedTagsAsync()
        {
            return await _tagRepository.GetUnusedTagsAsync();
        }

        public async Task<IEnumerable<TagDto>> GetMostUsedTagsAsync(int topCount = 10)
        {
            if (topCount <= 0)
            {
                throw new ArgumentException("Top count must be greater than 0.", nameof(topCount));
            }

            return await _tagRepository.GetMostUsedTagsAsync(topCount);
        }

        public async Task<TagStatisticsDto> GetTagStatisticsAsync()
        {
            var allTags = await _tagRepository.GetAllTagsAsync();
            var tagsList = allTags.ToList();

            var totalTags = tagsList.Count;
            var usedTags = tagsList.Count(t => t.ArticleCount > 0);
            var unusedTags = totalTags - usedTags;
            var totalArticlesTagged = tagsList.Sum(t => t.ArticleCount);
            var averageTagsPerArticle = usedTags > 0 ? (double)totalArticlesTagged / usedTags : 0;

            return new TagStatisticsDto
            {
                TotalTags = totalTags,
                UsedTags = usedTags,
                UnusedTags = unusedTags,
                TotalArticlesTagged = totalArticlesTagged,
                AverageTagsPerArticle = Math.Round(averageTagsPerArticle, 2)
            };
        }

        #endregion
    }
}












