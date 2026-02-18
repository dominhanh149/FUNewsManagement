public class NewsArticleReportDto
{
    public int TotalArticles { get; set; }

    public int TotalActive { get; set; }

    public int TotalInactive { get; set; }

    public List<CategoryStatDto> CategoryStats { get; set; } = new();

    public List<AuthorStatDto> AuthorStats { get; set; } = new();
}
