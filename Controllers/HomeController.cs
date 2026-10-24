using System.Diagnostics;
using EddaStore.Data;
using EddaStore.Models;
using EddaStore.Models.Products;
using EddaStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Take(8)
            .ToListAsync();

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity > 0)
            .Include(p => p.Images)
            .OrderByDescending(p => p.Name)
            .Take(8)
            .ToListAsync();

        foreach (var product in products)
        {
            product.Images = product.Images
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.SortOrder)
                .Take(1)
                .ToList();
        }

        var viewModel = new HomeViewModel
        {
            Categories = categories,
            FeaturedProducts = products
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Faq()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id
                ?? HttpContext.TraceIdentifier
        });
    }
}