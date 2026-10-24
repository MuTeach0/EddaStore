using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EddaStore.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class RecycleBinController(IRecycleBinService recycleBinService) : Controller
{
    public async Task<IActionResult> Index(string? entityType, string? search, CancellationToken cancellationToken)
    {
        ViewBag.EntityType = entityType;
        ViewBag.Search = search;
        return View(await recycleBinService.GetItemsAsync(entityType, search, cancellationToken));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(string entityType, Guid id, CancellationToken cancellationToken)
    {
        try { await recycleBinService.RestoreAsync(entityType, id, cancellationToken); TempData["Success"] = "تمت استعادة السجل بنجاح."; }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException ex) { TempData["Error"] = ex.Message; }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index), new { entityType });
    }
}
