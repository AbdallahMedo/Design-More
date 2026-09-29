using System.Security.Claims;
using MechanicalDesigns.Application.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MechanicalDesigns.Api.Controllers;
[ApiController][Authorize][Route("api/cart")]
public sealed class CartController(ICartService cart):ControllerBase
{
 private Guid UserId()=>Guid.Parse(User.FindFirstValue("sub")!);
 [HttpGet]public async Task<IActionResult> Get(CancellationToken ct)=>Ok(await cart.GetAsync(UserId(),ct));
 [HttpPost("items")]public async Task<IActionResult>Add(CartItemCreateRequest r,CancellationToken ct){try{return Ok(await cart.AddAsync(UserId(),r,ct));}catch(Exception e)when(e is ArgumentException or MechanicalDesigns.Application.Common.ConflictException){return BadRequest(new{success=false,message=e.Message});}}
 [HttpPut("items/{id:guid}")]public async Task<IActionResult>Update(Guid id,CartItemUpdateRequest r,CancellationToken ct){try{var x=await cart.UpdateAsync(UserId(),id,r,ct);return x is null?NotFound():Ok(x);}catch(Exception e)when(e is ArgumentException or MechanicalDesigns.Application.Common.ConflictException){return BadRequest(new{success=false,message=e.Message});}}
 [HttpDelete("items/{id:guid}")]public async Task<IActionResult>Remove(Guid id,CancellationToken ct)=>await cart.RemoveAsync(UserId(),id,ct)?NoContent():NotFound();
 [HttpDelete]public async Task<IActionResult>Clear(CancellationToken ct){await cart.ClearAsync(UserId(),ct);return NoContent();}
}
