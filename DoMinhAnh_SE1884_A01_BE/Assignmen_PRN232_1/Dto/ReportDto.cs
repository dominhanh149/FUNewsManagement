using Assignmen_PRN232_1.DTOs.Common;

namespace Assignmen_PRN232__.Dto
{
    public class NewsArticleReportDto
    {
        public int TotalArticles { get; set; }
        public int TotalActive { get; set; }
        public int TotalInactive { get; set; }
        public List<CategoryStatDto> CategoryStats { get; set; } = new();
        public List<AuthorStatDto> AuthorStats { get; set; } = new();
    }

    public class CategoryStatDto
    {
        public short? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int ArticleCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
    }

    public class AuthorStatDto
    {
        public short? AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public int ArticleCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
    }

    public class ReportSearchDto : BaseSearchDto
    {
        public short? CategoryId { get; set; }
        public short? AuthorId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
