using System.Diagnostics;
using EddaStore.Models;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EddaStore.Controllers;

public class HomeController(IHomeService homeService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await homeService.GetHomePageAsync(cancellationToken));

    public IActionResult Privacy() => View();
    public IActionResult Contact() => View();
    public IActionResult Faq() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
