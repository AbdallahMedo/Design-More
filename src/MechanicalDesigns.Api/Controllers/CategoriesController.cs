using MechanicalDesigns.Application.Categories;
using MechanicalDesigns.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicalDesigns.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(ICategoryService categories) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) => Ok(await categories.GetPublicAsync(cancellationToken));

    [HttpGet("{idOrSlug}")]
    public async Task<IActionResult> Get(string idOrSlug, CancellationToken cancellationToken)
    {
        var result = await categories.GetPublicAsync(idOrSlug, cancellationToken);
        return result is null ? NotFound(new { success = false, message = "Category not found." }) : Ok(result);
    }
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/categories")]
public sealed class AdminCategoriesController(ICategoryService categories) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) => Ok(await categories.GetAdminAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CategoryUpsertRequest request, CancellationToken cancellationToken)
    {
        if (!IsValid(request)) return BadRequest(new { success = false, message = "Arabic name, English name, and slug are required." });
        try { return StatusCode(StatusCodes.Status201Created, await categories.CreateAsync(request, cancellationToken)); }
        catch (ConflictException ex) { return Conflict(new { success = false, message = ex.Message }); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CategoryUpsertRequest request, CancellationToken cancellationToken)
    {
        if (!IsValid(request)) return BadRequest(new { success = false, message = "Arabic name, English name, and slug are required." });
        try { var result = await categories.UpdateAsync(id, request, cancellationToken); return result is null ? NotFound() : Ok(result); }
        catch (ConflictException ex) { return Conflict(new { success = false, message = ex.Message }); }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, CategoryStatusRequest request, CancellationToken cancellationToken) =>
        await categories.SetStatusAsync(id, request.IsActive, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await categories.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    private static bool IsValid(CategoryUpsertRequest request) => !string.IsNullOrWhiteSpace(request.NameAr) && !string.IsNullOrWhiteSpace(request.NameEn) && !string.IsNullOrWhiteSpace(request.Slug);
}
