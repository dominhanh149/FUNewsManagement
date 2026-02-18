using Assignmen_PRN232_1.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace Assignmen_PRN232__.Dto
{
    /// <summary>
    /// DTO for creating a new Tag
    /// </summary>
    public class CreateTagDto
    {
        public int TagID { get; set; }

        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(50, ErrorMessage = "Tag name cannot exceed 50 characters")]
        public string TagName { get; set; } = null!;

        [StringLength(400, ErrorMessage = "Note cannot exceed 400 characters")]
        public string? Note { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing Tag
    /// </summary>
    public class UpdateTagDto
    {
        [Required(ErrorMessage = "Tag ID is required")]
        public int TagID { get; set; }

        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(50, ErrorMessage = "Tag name cannot exceed 50 characters")]
        public string TagName { get; set; } = null!;

        [StringLength(400, ErrorMessage = "Note cannot exceed 400 characters")]
        public string? Note { get; set; }
    }

    /// <summary>
    /// DTO for Tag response
    /// </summary>
    public class TagDto
    {
        public int TagID { get; set; }
        public string TagName { get; set; } = null!;
        public string? Note { get; set; }
        public int ArticleCount { get; set; } // Number of articles using this tag
    }

    /// <summary>
    /// DTO for Tag with related articles
    /// </summary>
    public class TagWithArticlesDto
    {
        public int TagID { get; set; }
        public string TagName { get; set; } = null!;
        public string? Note { get; set; }
        public List<TagArticleDto> Articles { get; set; } = new List<TagArticleDto>();
    }

    /// <summary>
    /// DTO for article information in Tag context
    /// </summary>
    public class TagArticleDto
    {
        public string NewsArticleID { get; set; } = null!;
        public string? NewsTitle { get; set; }
        public string Headline { get; set; } = null!;
        public DateTime? CreatedDate { get; set; }
        public bool? NewsStatus { get; set; }
        public string? CategoryName { get; set; }
        public string? CreatedByName { get; set; }
    }

    /// <summary>
    /// DTO for Tag search parameters
    /// </summary>
    public class TagSearchDto : BaseSearchDto
    {
        public string? TagName { get; set; }
        public bool? HasArticles { get; set; } // Filter tags that have/don't have articles
    }

    /// <summary>
    /// DTO for paginated response
    /// </summary>
    public class PaginatedTagResponse
    {
        public IEnumerable<TagDto> Tags { get; set; } = new List<TagDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}












