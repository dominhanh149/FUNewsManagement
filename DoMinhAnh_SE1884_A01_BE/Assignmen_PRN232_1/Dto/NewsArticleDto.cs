using Assignmen_PRN232__.Models;
using Assignmen_PRN232_1.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace Assignmen_PRN232__.Dto
{
    public class NewsArticleDto
    {
        public string NewsArticleId { get; set; }

        public string NewsArticleName { get; set; }

        public string? NewsTitle { get; set; }

        public string Headline { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public string? NewsContent { get; set; }

        public string? NewsSource { get; set; }

        public short? CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public bool? NewsStatus { get; set; }

        public int ViewCount { get; set; }

        public short? CreatedById { get; set; }

        public string? CreatedByName { get; set; }

        public short? UpdatedById { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public ICollection<TagDto> Tags { get; set; } = new List<TagDto>();
    }

    public class NewsArticleSaveDto
    {
        public string? NewsArticleId { get; set; }

        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? NewsTitle { get; set; }

        [Required(ErrorMessage = "Headline is required")]
        [StringLength(500, ErrorMessage = "Headline cannot exceed 500 characters")]
        public string? Headline { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? NewsContent { get; set; }

        [StringLength(200, ErrorMessage = "Source cannot exceed 200 characters")]
        public string? NewsSource { get; set; }

        public short? CategoryId { get; set; }

        public bool? NewsStatus { get; set; }

        public short? CreatedById { get; set; }

        public short? UpdatedById { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public List<int> TagIds { get; set; }

    }

    public class NewsArticleSearchDto : BaseSearchDto
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public short? CategoryId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public short? CreatedById { get; set; }
    }
}
