using MechanicalDesigns.Application.Common;
using MechanicalDesigns.Application.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicalDesigns.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] ProductQuery query, CancellationToken ct) => Ok(await products.GetPublicAsync(query, ct));
    [HttpGet("{id:guid}")] public async Task<IActionResult> Get(Guid id, CancellationToken ct) => await Found(products.GetPublicAsync(id, ct));
    [HttpGet("slug/{slug}")] public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct) => await Found(products.GetPublicBySlugAsync(slug, ct));
    private async Task<IActionResult> Found(Task<ProductResponse?> task)
    {
        var value = await task;
        return value is null ? NotFound(new { success = false, message = "Product not found." }) : Ok(value);
    }
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/products")]
public sealed class AdminProductsController(IProductService products) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] ProductQuery query, CancellationToken ct) => Ok(await products.GetAdminAsync(query, ct));
    [HttpGet("{id:guid}")] public async Task<IActionResult> Get(Guid id, CancellationToken ct) { var product = await products.GetAdminAsync(id, ct); return product is null ? NotFound() : Ok(product); }
    [HttpPost] public async Task<IActionResult> Create(ProductUpsertRequest request, CancellationToken ct) { if (!Valid(request)) return BadRequest(); try { return StatusCode(201, await products.CreateAsync(request, ct)); } catch (ConflictException e) { return Conflict(new { success = false, message = e.Message }); } }
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, ProductUpsertRequest request, CancellationToken ct) { if (!Valid(request)) return BadRequest(); try { var r = await products.UpdateAsync(id, request, ct); return r is null ? NotFound() : Ok(r); } catch (ConflictException e) { return Conflict(new { success = false, message = e.Message }); } }
    [HttpPatch("{id:guid}/status")] public async Task<IActionResult> Status(Guid id, ProductStatusRequest r, CancellationToken ct) => await products.SetStatusAsync(id, r.IsActive, ct) ? NoContent() : NotFound();
    [HttpPatch("{id:guid}/stock")] public async Task<IActionResult> Stock(Guid id, ProductStockRequest r, CancellationToken ct) { if (r.StockQuantity < 0) return BadRequest(); return await products.SetStockAsync(id, r.StockQuantity, ct) ? NoContent() : NotFound(); }
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => await products.DeleteAsync(id, ct) ? NoContent() : NotFound();
    private static bool Valid(ProductUpsertRequest r) => r.CategoryId != Guid.Empty && !string.IsNullOrWhiteSpace(r.TitleAr) && !string.IsNullOrWhiteSpace(r.TitleEn) && !string.IsNullOrWhiteSpace(r.Slug) && r.Price >= 0 && r.StockQuantity >= 0;
}
