using Microsoft.AspNetCore.Mvc;

namespace FE.Controllers.Staff
{
    [Route("Staff/[controller]")]
    public class NewsController : Controller
    {
        private readonly IConfiguration _config;
        public NewsController(IConfiguration config) => _config = config;

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            ViewBag.ApiBaseUrl = _config["Api:BaseUrl"];
            return View("~/Views/Staff/News/Index.cshtml");
        }
    }
}
