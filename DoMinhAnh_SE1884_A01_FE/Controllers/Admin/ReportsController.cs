using Microsoft.AspNetCore.Mvc;

namespace FE.Controllers.Admin
{
    [Route("Admin/[controller]")]
    public class ReportsController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Reports/Index.cshtml");
        }
    }
}
