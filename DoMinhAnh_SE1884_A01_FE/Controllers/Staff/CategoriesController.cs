using Microsoft.AspNetCore.Mvc;

namespace FE.Controllers.Staff
{
    [Route("Staff/[controller]")]
    public class CategoriesController : Controller
    {
        private readonly IConfiguration _config;
        public CategoriesController(IConfiguration config) => _config = config;

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            ViewBag.ApiBaseUrl = _config["Api:BaseUrl"];
            return View("~/Views/Staff/Categories/Index.cshtml");
        }
    }
}
