using MechanicalDesigns.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace MechanicalDesigns.Api.Controllers;
[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/messages")]
[Route("api/admin/contact-messages")]
public sealed class AdminMessagesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1 || page > 1000000 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Invalid pagination parameters." });
        var totalCount = await db.ContactMessages.CountAsync(ct);
        var items = await db.ContactMessages.AsNoTracking().OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(new { items, totalCount, page, pageSize });
    }
    [HttpGet("count")]
    public async Task<IActionResult> Count(CancellationToken ct) => Ok(new { totalCount = await db.ContactMessages.CountAsync(ct) });

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var message = await db.ContactMessages.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id, ct);
        return message is null ? NotFound() : Ok(message);
    }
}
