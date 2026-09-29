using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicalDesigns.Api.Controllers;

/// <summary>Temporary policy-verification endpoint retained until admin modules are introduced.</summary>
[ApiController]
[Route("api/admin")]
public sealed class AuthorizationController : ControllerBase
{
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("authorization-check")]
    public IActionResult Check() => Ok(new { success = true, message = "Admin authorization granted." });
}
