using Microsoft.AspNetCore.Mvc;

namespace FE.Controllers.Admin
{
    [Route("Admin/[controller]")]
    public class AuditLogController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/AuditLog/Index.cshtml");
        }
    }
}
