using MechanicalDesigns.Application.Delivery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicalDesigns.Api.Controllers;

[ApiController]
[Route("api/delivery-governorates")]
public sealed class DeliveryGovernoratesController(IGovernorateDeliveryFeeService deliveryFees) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await deliveryFees.GetPublicAsync(ct));
}

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/delivery-governorates")]
public sealed class AdminDeliveryGovernoratesController(IGovernorateDeliveryFeeService deliveryFees) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await deliveryFees.GetAdminAsync(ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id,GovernorateDeliveryFeeUpdateRequest request,CancellationToken ct)
    {
        try { var result = await deliveryFees.UpdateAsync(id, request, ct); return result is null ? NotFound() : Ok(result); }
        catch (ArgumentException ex) { return BadRequest(new { success = false, message = ex.Message }); }
    }
}
