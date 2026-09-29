using System.Security.Claims;
using MechanicalDesigns.Application.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MechanicalDesigns.Api.Controllers;
[ApiController][Authorize(Policy="AdminOnly")][Route("api/admin/orders")]
public sealed class AdminOrdersController(IOrderService orders):ControllerBase
{
 [HttpGet]public async Task<IActionResult>Get([FromQuery]string? status,CancellationToken ct)=>Ok(await orders.GetAdminAsync(status,ct));
 [HttpGet("{id:guid}")]public async Task<IActionResult>Get(Guid id,CancellationToken ct){var x=await orders.GetAdminAsync(id,ct);return x is null?NotFound():Ok(x);}
 [HttpPatch("{id:guid}/status")]public async Task<IActionResult>Status(Guid id,OrderStatusUpdateRequest r,CancellationToken ct){try{return await orders.SetStatusAsync(id,Guid.Parse(User.FindFirstValue("sub")!),r,ct)?NoContent():NotFound();}catch(ArgumentException e){return BadRequest(new{success=false,message=e.Message});}}
 [HttpPatch("{id:guid}/payment-status")]public async Task<IActionResult>Payment(Guid id,PaymentStatusUpdateRequest r,CancellationToken ct){try{return await orders.SetPaymentStatusAsync(id,r,ct)?NoContent():NotFound();}catch(ArgumentException e){return BadRequest(new{success=false,message=e.Message});}}
 [HttpPatch("{orderId:guid}/items/{itemId:guid}/status")]public async Task<IActionResult>Item(Guid orderId,Guid itemId,OrderItemStatusUpdateRequest r,CancellationToken ct){try{return await orders.SetItemStatusAsync(orderId,itemId,r,ct)?NoContent():NotFound();}catch(ArgumentException e){return BadRequest(new{success=false,message=e.Message});}}
}
