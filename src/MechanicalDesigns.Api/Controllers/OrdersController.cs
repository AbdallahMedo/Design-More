using System.Security.Claims;
using MechanicalDesigns.Application.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MechanicalDesigns.Api.Controllers;
[ApiController][Authorize][Route("api/orders")]
public sealed class OrdersController(IOrderService orders):ControllerBase
{
 private Guid UserId()=>Guid.Parse(User.FindFirstValue("sub")!);
 [HttpPost]public async Task<IActionResult>Create(OrderCreateRequest r,CancellationToken ct){if(string.IsNullOrWhiteSpace(r.CustomerName)||string.IsNullOrWhiteSpace(r.PhoneNumber)||string.IsNullOrWhiteSpace(r.Address)||string.IsNullOrWhiteSpace(r.City)||string.IsNullOrWhiteSpace(r.Governorate))return BadRequest();try{return StatusCode(201,await orders.CreateAsync(UserId(),r,ct));}catch(MechanicalDesigns.Application.Common.ConflictException e){return BadRequest(new{success=false,message=e.Message});}}
 [HttpGet]public async Task<IActionResult>Get(CancellationToken ct)=>Ok(await orders.GetMineAsync(UserId(),ct));
 [HttpGet("{id:guid}")]public async Task<IActionResult>Get(Guid id,CancellationToken ct){var x=await orders.GetMineAsync(UserId(),id,ct);return x is null?NotFound():Ok(x);}
}
