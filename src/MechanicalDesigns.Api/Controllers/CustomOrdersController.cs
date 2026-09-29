using System.Security.Claims;
using MechanicalDesigns.Application.CustomOrders;
using MechanicalDesigns.Application.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicalDesigns.Api.Controllers;

[ApiController]
[Route("api/custom-orders")]
public sealed class CustomOrdersController(ICustomOrderService service, IFileStorageService storage) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CustomOrderCreateRequest request, CancellationToken ct)
    {
        if (!IsValid(request))
            return BadRequest();

        Guid? userId = Guid.TryParse(User.FindFirstValue("sub"), out var id) ? id : null;
        return StatusCode(201, await service.CreateAsync(userId, request, ct));
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> Mine(CancellationToken ct) => Ok(await service.MineAsync(CurrentUserId(), ct));

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMine(Guid id, CancellationToken ct)
    {
        var result = await service.GetMineAsync(id, CurrentUserId(), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize]
    [HttpPost("submit")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> Submit([FromForm] CustomOrderCreateRequest request, IFormFile? image, [FromForm] int displayOrder, CancellationToken ct)
    {
        if (!IsValid(request)) return BadRequest(new { success = false, message = "Name, email, WhatsApp number, and description are required." });
        if (image is not null && (image.Length == 0 || image.Length > 5_000_000)) return BadRequest(new { success = false, message = "An optional image must be no larger than 5 MB." });
        if (image is not null && image.ContentType is not "image/jpeg" and not "image/png" and not "image/webp") return BadRequest(new { success = false, message = "Only JPEG, PNG, and WebP images are allowed." });

        var userId = CurrentUserId();
        var order = await service.CreateAsync(userId, request, ct);
        if (image is null) return StatusCode(201, order);

        await using var input = image.OpenReadStream();
        var imageUrl = await storage.SaveImageAsync(input, image.FileName, image.ContentType, ct);
        var savedImage = await service.AddImageAsync(order.Id, userId, imageUrl, displayOrder, ct);
        return StatusCode(201, order with { Images = savedImage is null ? [] : [savedImage] });
    }

    [Authorize]
    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages(Guid id, CancellationToken ct)
    {
        var result = await service.GetImagesAsync(id, CurrentUserId(), false, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/images/upload")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, [FromForm] int displayOrder, CancellationToken ct)
    {
        if (file is null || file.Length == 0 || file.Length > 5_000_000)
            return BadRequest(new { success = false, message = "An image up to 5 MB is required." });
        if (file.ContentType is not "image/jpeg" and not "image/png" and not "image/webp")
            return BadRequest(new { success = false, message = "Only JPEG, PNG, and WebP images are allowed." });

        try
        {
            await using var input = file.OpenReadStream();
            var imageUrl = await storage.SaveImageAsync(input, file.FileName, file.ContentType, ct);
            var result = await service.AddImageAsync(id, CurrentUserId(), imageUrl, displayOrder, ct);
            return result is null ? NotFound() : StatusCode(201, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{orderId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid orderId, Guid imageId, CancellationToken ct) =>
        await service.DeleteImageAsync(orderId, imageId, CurrentUserId(), false, ct) ? NoContent() : NotFound();

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue("sub")!);
    private static bool IsValid(CustomOrderCreateRequest request) =>
        !string.IsNullOrWhiteSpace(request.Name) && !string.IsNullOrWhiteSpace(request.Email) && request.Email.Contains('@') &&
        !string.IsNullOrWhiteSpace(request.WhatsAppNumber) && !string.IsNullOrWhiteSpace(request.Description);
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/custom-orders")]
public sealed class AdminCustomOrdersController(ICustomOrderService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? status, CancellationToken ct) => Ok(await service.AdminAsync(status, ct));

    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages(Guid id, CancellationToken ct)
    {
        var result = await service.GetImagesAsync(id, null, true, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> Status(Guid id, CustomOrderStatusRequest request, CancellationToken ct)
    {
        try { return await service.StatusAsync(id, request.Status, ct) ? NoContent() : NotFound(); }
        catch (ArgumentException ex) { return BadRequest(new { success = false, message = ex.Message }); }
    }
}
