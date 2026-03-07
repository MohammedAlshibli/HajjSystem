using Microsoft.AspNetCore.Mvc;

namespace HajjSystem.Web.Controllers;

public class HomeController : BaseController
{
    public IActionResult Index() => View();
    public IActionResult Unauthorised() => View();
}
