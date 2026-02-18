using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Repositories.IRepositories;
using Assignmen_PRN232_1.Services.IServices;

namespace Assignmen_PRN232_1.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;

        public ReportService(IReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<NewsArticleReportDto> GetNewsArticleReportAsync(ReportSearchDto searchDto)
        {
            // đảm bảo FromDate <= ToDate nếu user nhập ngược
            if (searchDto.FromDate.HasValue && searchDto.ToDate.HasValue
                && searchDto.FromDate.Value > searchDto.ToDate.Value)
            {
                // swap
                var tmp = searchDto.FromDate;
                searchDto.FromDate = searchDto.ToDate;
                searchDto.ToDate = tmp;
            }

            return await _repo.GetNewsArticleReportAsync(searchDto);
        }
    }
}
