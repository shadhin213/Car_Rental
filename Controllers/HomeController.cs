using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Data;

namespace CarRentalManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext? _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext? context = null)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var userName = HttpContext.Session.GetString("UserName");
        ViewBag.UserName = userName;
        
        return View();
    }

    public IActionResult AvailableCars()
    {
        return View();
    }

    public IActionResult Services()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
