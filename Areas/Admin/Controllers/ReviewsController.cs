using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EddaStore.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class ReviewsController(IReviewAdminService service) : Controller { public async Task<IActionResult> Index(CancellationToken ct) => View(await service.GetAllAsync(ct)); [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> SetApproval(Guid id, bool approved, CancellationToken ct) { try { await service.SetApprovalAsync(id, approved, ct); TempData["Success"] = "تم تحديث حالة المراجعة."; } catch (KeyNotFoundException) { return NotFound(); } return RedirectToAction(nameof(Index)); } [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { try { await service.DeleteAsync(id, ct); TempData["Success"] = "تم حذف المراجعة."; } catch (KeyNotFoundException) { return NotFound(); } return RedirectToAction(nameof(Index)); } }
