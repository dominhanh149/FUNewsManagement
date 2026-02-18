using Microsoft.AspNetCore.Mvc;

namespace FE.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        public HomeController(IConfiguration config) => _config = config;

        public IActionResult Index()
        {
            ViewBag.ApiBaseUrl = _config["Api:BaseUrl"];
            return View();
        }
    }
}
