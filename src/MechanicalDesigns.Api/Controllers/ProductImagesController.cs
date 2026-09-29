using MechanicalDesigns.Application.Products;
using MechanicalDesigns.Application.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicalDesigns.Api.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/images")]
public sealed class ProductImagesController(IProductImageService images) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid productId, CancellationToken ct)
    {
        var result = await images.GetAsync(productId, true, ct);
        return result.Count == 0 ? Ok(result) : Ok(result);
    }
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/products/{productId:guid}/images")]
public sealed class AdminProductImagesController(IProductImageService images, IFileStorageService storage) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(Guid productId, ProductImageCreateRequest request, CancellationToken ct)
    {
        try { var result = await images.CreateAsync(productId, request, ct); return result is null ? NotFound() : StatusCode(201, result); }
        catch (ArgumentException ex) { return BadRequest(new { success = false, message = ex.Message }); }
    }
    [HttpDelete("{imageId:guid}")]
    public async Task<IActionResult> Delete(Guid productId, Guid imageId, CancellationToken ct) => await images.DeleteAsync(productId, imageId, ct) ? NoContent() : NotFound();

    [HttpPost("upload")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> Upload(Guid productId, IFormFile file, [FromForm] int displayOrder, [FromForm] bool isPrimary, CancellationToken ct)
    {
        if (file is null || file.Length == 0 || file.Length > 5_000_000) return BadRequest(new { success = false, message = "An image up to 5 MB is required." });
        if (file.ContentType is not "image/jpeg" and not "image/png" and not "image/webp") return BadRequest(new { success = false, message = "Only JPEG, PNG, and WebP images are allowed." });
        try { await using var input = file.OpenReadStream(); var url = await storage.SaveImageAsync(input, file.FileName, file.ContentType, ct); var result = await images.CreateAsync(productId, new ProductImageCreateRequest(url, displayOrder, isPrimary), ct); return result is null ? NotFound() : StatusCode(201, result); }
        catch (ArgumentException ex) { return BadRequest(new { success = false, message = ex.Message }); }
    }
}
