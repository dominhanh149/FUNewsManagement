using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232_1.DTOs.Common;

namespace Assignmen_PRN232__.Repositories.IRepositories
{
    /// <summary>
    /// Repository interface for Tag entity operations
    /// </summary>
    public interface ITagRepository
    {
        #region Basic CRUD Operations

        /// <summary>
        /// Get all tags with article count
        /// </summary>
        Task<IEnumerable<TagDto>> GetAllTagsAsync();

        /// <summary>
        /// Get tag by ID
        /// </summary>
        Task<Tag?> GetTagByIdAsync(int tagId);

        /// <summary>
        /// Get tag with all related articles
        /// </summary>
        Task<TagWithArticlesDto?> GetTagWithArticlesAsync(int tagId);

        /// <summary>
        /// Create a new tag
        /// </summary>
        Task<Tag> CreateTagAsync(CreateTagDto createTagDto);

        /// <summary>
        /// Update an existing tag
        /// </summary>
        Task<bool> UpdateTagAsync(UpdateTagDto updateTagDto);

        /// <summary>
        /// Delete a tag (only if not used in any articles)
        /// </summary>
        Task<bool> DeleteTagAsync(int tagId);

        #endregion

        #region Validation and Check Methods

        /// <summary>
        /// Check if tag exists by ID
        /// </summary>
        Task<bool> TagExistsAsync(int tagId);

        /// <summary>
        /// Check if tag name already exists (for different tag)
        /// </summary>
        Task<bool> TagNameExistsAsync(string tagName, int? excludeTagId = null);

        /// <summary>
        /// Check if tag is used in any articles
        /// </summary>
        Task<bool> IsTagUsedInArticlesAsync(int tagId);

        /// <summary>
        /// Get count of articles using this tag
        /// </summary>
        Task<int> GetArticleCountByTagAsync(int tagId);

        #endregion

        #region Search and Filter Methods

        /// <summary>
        /// Search tags with pagination and filtering
        /// </summary>
        Task<(IEnumerable<TagDto> Tags, int TotalCount)> SearchTagsAsync(TagSearchDto searchDto);

        /// <summary>
        /// Get all articles using a specific tag
        /// </summary>
        Task<IEnumerable<TagArticleDto>> GetArticlesByTagAsync(int tagId);

        /// <summary>
        /// Get tags by article ID
        /// </summary>
        Task<IEnumerable<TagDto>> GetTagsByArticleIdAsync(string articleId);

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Get multiple tags by IDs
        /// </summary>
        Task<IEnumerable<Tag>> GetTagsByIdsAsync(List<int> tagIds);

        /// <summary>
        /// Get unused tags (tags not assigned to any article)
        /// </summary>
        Task<IEnumerable<TagDto>> GetUnusedTagsAsync();

        /// <summary>
        /// Get most used tags with article count
        /// </summary>
        Task<IEnumerable<TagDto>> GetMostUsedTagsAsync(int topCount = 10);

        #endregion
    }
}