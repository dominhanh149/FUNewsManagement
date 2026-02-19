using Microsoft.AspNetCore.Mvc;

namespace FE.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        // GET /News/Detail/{id}
        [Route("News/Detail/{id}")]
        public IActionResult Detail(string id)
        {
            ViewBag.ArticleId = id;
            return View("~/Views/Home/Detail.cshtml");
        }
    }
}
