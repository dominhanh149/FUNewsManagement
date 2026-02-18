public class ReportFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public short? CategoryId { get; set; }
    public short? AuthorId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
