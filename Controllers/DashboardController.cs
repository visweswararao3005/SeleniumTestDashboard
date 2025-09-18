using Microsoft.AspNetCore.Mvc;
[Route("[controller]")]
public class DashboardController : Controller
{
    [HttpGet("")]
    [HttpGet("Index")] // allow both /Dashboard and /Dashboard/Index
    public IActionResult Index() => View("~/Views/Dashboard/index.cshtml");

    [HttpGet("Danyab")]
    public IActionResult Danyab()
    {
        ViewBag.Client = "Danya B";
        return View("~/Views/Dashboard/index.cshtml");
    }
    [HttpGet("Bestpet")]
    public IActionResult BestPet()
    {
        ViewBag.Client = "BestPet";
        return View("~/Views/Dashboard/index.cshtml");
    }
    [HttpGet("Capital")]
    public IActionResult Capital()
    {
        ViewBag.Client = "Capital";
        return View("~/Views/Dashboard/index.cshtml");
    }

}
