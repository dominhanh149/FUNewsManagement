using Assignmen_PRN232__.Dto;

namespace Assignmen_PRN232_1.Services.IServices
{
    public interface IReportService
    {
        Task<NewsArticleReportDto> GetNewsArticleReportAsync(ReportSearchDto searchDto);
    }
}
