using MechanicalDesigns.Application.Advertisements;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace MechanicalDesigns.Api.Controllers;
[ApiController][Route("api/advertisements")]public sealed class AdvertisementsController(IAdvertisementService s):ControllerBase{[HttpGet]public async Task<IActionResult>Get(CancellationToken ct)=>Ok(await s.PublicAsync(ct));}
[ApiController][Authorize(Policy="AdminOnly")][Route("api/admin/advertisements")]public sealed class AdminAdvertisementsController(IAdvertisementService s):ControllerBase{
 [HttpGet]public async Task<IActionResult>Get(CancellationToken ct)=>Ok(await s.AdminAsync(ct));
 [HttpPost]public async Task<IActionResult>Create(AdvertisementRequest r,CancellationToken ct){try{return StatusCode(201,await s.CreateAsync(r,ct));}catch(ArgumentException e){return BadRequest(new{success=false,message=e.Message});}}
 [HttpPut("{id:guid}")]public async Task<IActionResult>Update(Guid id,AdvertisementRequest r,CancellationToken ct){try{var x=await s.UpdateAsync(id,r,ct);return x is null?NotFound():Ok(x);}catch(ArgumentException e){return BadRequest(new{success=false,message=e.Message});}}
 [HttpPatch("{id:guid}/status")]public async Task<IActionResult>Status(Guid id,AdvertisementStatusRequest r,CancellationToken ct)=>await s.StatusAsync(id,r.IsActive,ct)?NoContent():NotFound();
 [HttpDelete("{id:guid}")]public async Task<IActionResult>Delete(Guid id,CancellationToken ct)=>await s.DeleteAsync(id,ct)?NoContent():NotFound();}
