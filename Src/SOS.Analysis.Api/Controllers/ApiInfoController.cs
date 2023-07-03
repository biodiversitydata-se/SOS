using Microsoft.AspNetCore.Mvc;

namespace SOS.Analysis.Api.Controllers
{
    public class ApiInfoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
