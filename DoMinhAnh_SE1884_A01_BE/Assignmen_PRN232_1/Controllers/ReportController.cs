using Assignmen_PRN232__.Dto;
using Assignmen_PRN232_1.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignmen_PRN232_1.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportsController(IReportService service)
        {
            _service = service;
        }

        // POST: api/reports/news-articles
        [HttpPost("news-articles")]
        public async Task<IActionResult> NewsArticleReport([FromBody] ReportSearchDto dto)
        {
            var result = await _service.GetNewsArticleReportAsync(dto);
            return Ok(result);
        }
    }
}
