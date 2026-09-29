using MechanicalDesigns.Application.Admin;using MechanicalDesigns.Application.Common;using MechanicalDesigns.Application.Orders;using MechanicalDesigns.Application.Reviews;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace MechanicalDesigns.Api.Controllers;
[ApiController][Authorize(Policy="AdminOnly")][Route("api/admin/users")]public sealed class AdminUsersController(IAdminUserService users,IOrderService orders,IReviewService reviews):ControllerBase{
 [HttpGet]public async Task<IActionResult>List([FromQuery]string? search,CancellationToken ct)=>Ok(await users.ListAsync(search,ct));
 [HttpGet("{id:guid}")]public async Task<IActionResult>Get(Guid id,CancellationToken ct){var x=await users.GetAsync(id,ct);return x is null?NotFound():Ok(x);}
 [HttpPost("admins")]public async Task<IActionResult>CreateAdmin(AdminCreateRequest r,CancellationToken ct){try{return StatusCode(201,await users.CreateAdminAsync(r,ct));}catch(ConflictException e){return Conflict(new{success=false,message=e.Message});}catch(ArgumentException e){return BadRequest(new{success=false,message=e.Message});}}
 [HttpPatch("{id:guid}/block")]public async Task<IActionResult>Block(Guid id,CancellationToken ct)=>await users.BlockAsync(id,true,ct)?NoContent():NotFound();
 [HttpPatch("{id:guid}/unblock")]public async Task<IActionResult>Unblock(Guid id,CancellationToken ct)=>await users.BlockAsync(id,false,ct)?NoContent():NotFound();
 [HttpGet("{id:guid}/orders")]public async Task<IActionResult>Orders(Guid id,CancellationToken ct)=>Ok(await orders.GetMineAsync(id,ct));
 [HttpGet("{id:guid}/reviews")]public async Task<IActionResult>Reviews(Guid id,CancellationToken ct)=>Ok((await reviews.GetAdminAsync(new ReviewQuery(null,null,1,100),ct)).Where(x=>x.UserId==id));}
