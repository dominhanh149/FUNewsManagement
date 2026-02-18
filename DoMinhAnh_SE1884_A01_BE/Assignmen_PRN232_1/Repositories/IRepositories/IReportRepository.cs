using Assignmen_PRN232__.Dto;

namespace Assignmen_PRN232__.Repositories.IRepositories
{
    public interface IReportRepository
    {
        Task<NewsArticleReportDto> GetNewsArticleReportAsync(ReportSearchDto searchDto);
    }
}
