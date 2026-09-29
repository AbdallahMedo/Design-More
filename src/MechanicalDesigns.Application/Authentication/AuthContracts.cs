namespace MechanicalDesigns.Application.Authentication;

public sealed record RegisterRequest(string Name, string Email, string Password, string? PhoneNumber, string? WhatsAppNumber);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record VerifyEmailRequest(string Email,string Otp);public sealed record ForgotPasswordRequest(string Email);public sealed record ResetPasswordRequest(string Email,string Otp,string NewPassword);public sealed record ChangePasswordRequest(string CurrentPassword,string NewPassword);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt, UserResponse User);
public sealed record UserResponse(Guid Id, string Name, string Email, string? PhoneNumber, string? WhatsAppNumber, bool EmailVerified, string Role);

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);
    Task VerifyEmailAsync(VerifyEmailRequest request,CancellationToken ct);Task ResendVerificationOtpAsync(ForgotPasswordRequest request,CancellationToken ct);Task ForgotPasswordAsync(ForgotPasswordRequest request,CancellationToken ct);Task ResendPasswordResetOtpAsync(ForgotPasswordRequest request,CancellationToken ct);Task ResetPasswordAsync(ResetPasswordRequest request,CancellationToken ct);Task ChangePasswordAsync(Guid userId,ChangePasswordRequest request,CancellationToken ct);
}
