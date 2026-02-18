using Microsoft.AspNetCore.Mvc;

namespace FE.Controllers.Staff
{
    [Route("Staff/[controller]")]
    public class TagsController : Controller
    {
        private readonly IConfiguration _config;
        public TagsController(IConfiguration config) => _config = config;

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            ViewBag.ApiBaseUrl = _config["Api:BaseUrl"];
            return View("~/Views/Staff/Tags/Index.cshtml");
        }
    }
}
