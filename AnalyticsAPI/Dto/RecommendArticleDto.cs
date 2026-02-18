public class RecommendArticleDto
{
    public string NewsArticleId { get; set; } = null!;
    public string? NewsTitle { get; set; }
    public string? Headline { get; set; }
    public string? CategoryName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int ViewCount { get; set; }
}
