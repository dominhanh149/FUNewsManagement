namespace Assignmen_PRN232_1.DTOs.Common
{
    public class BaseSearchDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string SortDirection { get; set; } = "asc";
        public int Skip => (PageNumber - 1) * PageSize;
        public bool? Status { get; set; }

    }
}
