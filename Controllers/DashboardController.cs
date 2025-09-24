using Microsoft.AspNetCore.Mvc;
using TestDashboard.Helper;
using TestDashboard.Models;
[Route("[controller]")]
public class DashboardController : Controller
{
    private readonly ClientsHelper _clientsHelper;

    public DashboardController(ClientsHelper clientsHelper)
    {
        _clientsHelper = clientsHelper;
    }
    [HttpGet("")]
    [HttpGet("Index")] // allow both /Dashboard and /Dashboard/Index
    public IActionResult Index()
    {
        ViewBag.Clients = _clientsHelper.GetClients();

        return View("~/Views/Dashboard/index.cshtml");
    }
    [HttpGet("Scheduler")]
    public IActionResult Scheduler()
    {
        ViewBag.Clients = _clientsHelper.GetClients();
        ViewBag.TestNames = _clientsHelper.GetTestNames();

        return View("~/Views/Scheduler/scheduler.cshtml");
    }
}