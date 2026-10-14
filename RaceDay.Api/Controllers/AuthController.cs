using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Api.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
