using System.Security.Claims;
using MechanicalDesigns.Application.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicalDesigns.Api.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/reviews")]
public sealed class ReviewsController(IReviewService reviews) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Get(Guid productId, CancellationToken ct) => Ok(await reviews.GetPublicAsync(productId, ct));
    [Authorize]
    [HttpPost] public async Task<IActionResult> Create(Guid productId, ReviewCreateRequest r, CancellationToken ct) { if (!UserId(out var id)) return Unauthorized(); try { return StatusCode(201, await reviews.CreateAsync(productId,id,r,ct)); } catch (Exception e) when (e is ArgumentException or MechanicalDesigns.Application.Common.ConflictException) { return BadRequest(new {success=false,message=e.Message}); } }
    [Authorize]
    [HttpPut("{reviewId:guid}")] public async Task<IActionResult> Update(Guid productId, Guid reviewId, ReviewUpdateRequest r, CancellationToken ct) { if(!UserId(out var id))return Unauthorized(); try { var x=await reviews.UpdateAsync(productId,reviewId,id,r,ct);return x is null?NotFound():Ok(x); } catch(ArgumentException e){return BadRequest(new {success=false,message=e.Message});} }
    [Authorize]
    [HttpDelete("{reviewId:guid}")] public async Task<IActionResult> Delete(Guid productId,Guid reviewId,CancellationToken ct) { if(!UserId(out var id))return Unauthorized(); return await reviews.DeleteAsync(productId,reviewId,id,User.IsInRole("Admin"),ct)?NoContent():NotFound(); }
    private bool UserId(out Guid id) => Guid.TryParse(User.FindFirstValue("sub"), out id);
}

[ApiController]
[Authorize(Policy="AdminOnly")]
[Route("api/admin/reviews")]
public sealed class AdminReviewsController(IReviewService reviews) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Get([FromQuery] ReviewQuery q,CancellationToken ct)=>Ok(await reviews.GetAdminAsync(q,ct));
    [HttpPatch("{id:guid}/approve")] public async Task<IActionResult> Approve(Guid id,CancellationToken ct)=>await reviews.SetStatusAsync(id,"Approved",ct)?NoContent():NotFound();
    [HttpPatch("{id:guid}/reject")] public async Task<IActionResult> Reject(Guid id,CancellationToken ct)=>await reviews.SetStatusAsync(id,"Rejected",ct)?NoContent():NotFound();
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id,CancellationToken ct)=>await reviews.DeleteAsync(Guid.Empty,id,Guid.Empty,true,ct)?NoContent():NotFound();
}
