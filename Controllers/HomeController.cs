using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Trayectos_Especiales.Models;

namespace Trayectos_Especiales.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userName = HttpContext.Session.GetString("UserName");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null)
        {
            // Si no hay sesión activa, redirige al login
            return RedirectToAction("Login", "Account");
        }

        ViewBag.UserName = userName;
        ViewBag.UserRole = userRole;

        return View();
    }

    public IActionResult Privacy()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userName = HttpContext.Session.GetString("UserName");
        var userRole = HttpContext.Session.GetString("UserRole");

        ViewBag.UserId = userId;
        ViewBag.UserName = userName;
        ViewBag.UserRole = userRole;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
