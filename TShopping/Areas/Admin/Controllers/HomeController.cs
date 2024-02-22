using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TShopping.Helpers;

namespace TShopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = MyRole.Employee)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
