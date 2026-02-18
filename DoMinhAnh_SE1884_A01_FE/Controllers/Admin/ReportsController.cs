using Microsoft.AspNetCore.Mvc;

namespace FE.Controllers.Admin
{
    [Route("Admin/[controller]")]
    public class ReportsController : Controller
    {
        private readonly IConfiguration _config;
        public ReportsController(IConfiguration config) => _config = config;
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            ViewBag.ApiBaseUrl = _config["Api:BaseUrl"];
            return View("~/Views/Admin/Reports/Index.cshtml");
        }
    }
}
