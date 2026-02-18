using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Dto.Common;
using Assignmen_PRN232_1.DTOs.Common;

namespace Assignmen_PRN232_1.Services.IServices
{
    public interface ITagService
    {
        #region Basic CRUD Operations

        /// <summary>
        /// Get all tags with article count
        /// </summary>
        Task<IEnumerable<TagDto>> GetAllTagsAsync();

        /// <summary>
        /// Get tag by ID
        /// </summary>
        Task<TagDto?> GetTagByIdAsync(int tagId);

        /// <summary>
        /// Get tag with all related articles
        /// </summary>
        Task<TagWithArticlesDto?> GetTagWithArticlesAsync(int tagId);

        /// <summary>
        /// Create a new tag
        /// </summary>
        Task<TagDto> CreateTagAsync(CreateTagDto createTagDto);

        /// <summary>
        /// Update an existing tag
        /// </summary>
        Task<bool> UpdateTagAsync(UpdateTagDto updateTagDto);

        /// <summary>
        /// Delete a tag
        /// </summary>
        Task<bool> DeleteTagAsync(int tagId);

        #endregion

        #region Search and Filter

        /// <summary>
        /// Search tags with pagination and filtering
        /// </summary>
        Task<PaginatedTagResponse> SearchTagsAsync(TagSearchDto searchDto);

        /// <summary>
        /// Get all articles using a specific tag
        /// </summary>
        Task<IEnumerable<TagArticleDto>> GetArticlesByTagAsync(int tagId);

        /// <summary>
        /// Get tags by article ID
        /// </summary>
        Task<IEnumerable<TagDto>> GetTagsByArticleIdAsync(string articleId);

        #endregion

        #region Validation

        /// <summary>
        /// Check if tag exists
        /// </summary>
        Task<bool> TagExistsAsync(int tagId);

        /// <summary>
        /// Check if tag name is available
        /// </summary>
        Task<bool> IsTagNameAvailableAsync(string tagName, int? excludeTagId = null);

        /// <summary>
        /// Check if tag can be deleted
        /// </summary>
        Task<(bool CanDelete, string? Reason)> CanDeleteTagAsync(int tagId);

        #endregion

        #region Statistics

        /// <summary>
        /// Get unused tags
        /// </summary>
        Task<IEnumerable<TagDto>> GetUnusedTagsAsync();

        /// <summary>
        /// Get most used tags
        /// </summary>
        Task<IEnumerable<TagDto>> GetMostUsedTagsAsync(int topCount = 10);

        /// <summary>
        /// Get tag statistics
        /// </summary>
        Task<TagStatisticsDto> GetTagStatisticsAsync();

        #endregion
    }

    /// <summary>
    /// DTO for tag statistics
    /// </summary>
    public class TagStatisticsDto
    {
        public int TotalTags { get; set; }
        public int UsedTags { get; set; }
        public int UnusedTags { get; set; }
        public int TotalArticlesTagged { get; set; }
        public double AverageTagsPerArticle { get; set; }
    }
}












