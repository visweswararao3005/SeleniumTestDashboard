﻿using Microsoft.AspNetCore.Mvc;

public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
