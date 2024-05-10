using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvcShopApp.Models;
using MvcShopApp.Data;
using Microsoft.EntityFrameworkCore;

namespace MvcShopApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var catalogItems = await _context.CatalogItems.ToListAsync();
        return View(catalogItems);
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
