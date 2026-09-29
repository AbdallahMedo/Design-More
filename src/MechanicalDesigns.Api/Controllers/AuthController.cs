using System.Security.Claims;
using MechanicalDesigns.Application.Authentication;
using MechanicalDesigns.Application.Common;
using MechanicalDesigns.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MechanicalDesigns.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth, ApplicationDbContext db) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || request.Password.Length < 8)
            return BadRequest(new { success = false, message = "Name, a valid email, and a password of at least 8 characters are required." });
        try { return StatusCode(StatusCodes.Status201Created, await auth.RegisterAsync(request, cancellationToken)); }
        catch (ConflictException ex) { return Conflict(new { success = false, message = ex.Message }); }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await auth.LoginAsync(request, cancellationToken)); }
        catch (UnauthorizedException ex) { return Unauthorized(new { success = false, message = ex.Message }); }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await auth.RefreshAsync(request, cancellationToken)); }
        catch (UnauthorizedException ex) { return Unauthorized(new { success = false, message = ex.Message }); }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await auth.LogoutAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("verify-email")] public async Task<IActionResult> Verify(VerifyEmailRequest r,CancellationToken ct){try{await auth.VerifyEmailAsync(r,ct);return NoContent();}catch(UnauthorizedException e){return BadRequest(new{success=false,message=e.Message});}}
    [HttpPost("resend-verification-otp")] public async Task<IActionResult> ResendVerificationOtp(ForgotPasswordRequest r,CancellationToken ct){await auth.ResendVerificationOtpAsync(r,ct);return Ok(new{success=true,message="If the account exists and is not verified, a verification OTP has been sent."});}
    [HttpPost("forgot-password")] public async Task<IActionResult> Forgot(ForgotPasswordRequest r,CancellationToken ct){await auth.ForgotPasswordAsync(r,ct);return Ok(new{success=true,message="If the account exists, a password reset OTP has been sent."});}
    [HttpPost("resend-password-reset-otp")] public async Task<IActionResult> ResendPasswordResetOtp(ForgotPasswordRequest r,CancellationToken ct){await auth.ResendPasswordResetOtpAsync(r,ct);return Ok(new{success=true,message="If the account exists, a password reset OTP has been sent."});}
    [HttpPost("reset-password")] public async Task<IActionResult> Reset(ResetPasswordRequest r,CancellationToken ct){try{await auth.ResetPasswordAsync(r,ct);return NoContent();}catch(Exception e)when(e is UnauthorizedException or ArgumentException){return BadRequest(new{success=false,message=e.Message});}}
    [Authorize][HttpPost("change-password")] public async Task<IActionResult> Change(ChangePasswordRequest r,CancellationToken ct){try{await auth.ChangePasswordAsync(Guid.Parse(User.FindFirstValue("sub")!),r,ct);return NoContent();}catch(Exception e)when(e is UnauthorizedException or ArgumentException){return BadRequest(new{success=false,message=e.Message});}}

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var id)) return Unauthorized();
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return user is null ? Unauthorized() : Ok(new UserResponse(user.Id, user.Name, user.Email, user.PhoneNumber, user.WhatsAppNumber, user.EmailVerified, user.Role.ToString()));
    }
}
