using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MechanicalDesigns.Application.Authentication;
using MechanicalDesigns.Application.Common;
using MechanicalDesigns.Domain.Entities;
using MechanicalDesigns.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MechanicalDesigns.Application.Email;

namespace MechanicalDesigns.Infrastructure.Authentication;

public sealed class AuthService(ApplicationDbContext db, IConfiguration configuration, IEmailService email) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, cancellationToken))
            throw new ConflictException("An account with this email already exists.");

        var user = new User
        {
            Name = request.Name.Trim(), Email = email, PasswordHash = HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber?.Trim(), WhatsAppNumber = request.WhatsAppNumber?.Trim()
        };
        db.Users.Add(user);
        var response=await CreateSessionAsync(user, cancellationToken);await SendVerificationAsync(user,cancellationToken);return response;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !VerifyPassword(request.Password, user.PasswordHash) || user.IsBlocked)
            throw new UnauthorizedException("Invalid email or password.");
        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        var hash = HashToken(request.RefreshToken);
        var existing = await db.RefreshTokens.Include(x => x.User).SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (existing is null || existing.RevokedAt is not null || existing.ExpiresAt <= DateTimeOffset.UtcNow || existing.User.IsBlocked)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        existing.RevokedAt = DateTimeOffset.UtcNow;
        var response = await CreateSessionAsync(existing.User, cancellationToken);
        existing.ReplacedByTokenHash = HashToken(response.RefreshToken);
        await db.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        var token = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == HashToken(request.RefreshToken), cancellationToken);
        if (token is not null && token.RevokedAt is null)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
    }
    public async Task VerifyEmailAsync(VerifyEmailRequest r,CancellationToken ct){if(!IsOtp(r.Otp))throw new UnauthorizedException("Invalid verification request.");var u=await db.Users.SingleOrDefaultAsync(x=>x.Email==r.Email.Trim().ToLowerInvariant(),ct)??throw new UnauthorizedException("Invalid verification request.");var t=await db.EmailVerificationTokens.SingleOrDefaultAsync(x=>x.UserId==u.Id&&x.TokenHash==HashToken(r.Otp)&&x.UsedAt==null&&x.ExpiresAt>DateTimeOffset.UtcNow,ct)??throw new UnauthorizedException("Invalid verification request.");t.UsedAt=DateTimeOffset.UtcNow;u.EmailVerified=true;await db.SaveChangesAsync(ct);}
    public async Task ResendVerificationOtpAsync(ForgotPasswordRequest r,CancellationToken ct){var u=await db.Users.SingleOrDefaultAsync(x=>x.Email==r.Email.Trim().ToLowerInvariant(),ct);if(u is null||u.EmailVerified)return;await SendVerificationAsync(u,ct);}
    public async Task ForgotPasswordAsync(ForgotPasswordRequest r,CancellationToken ct){var u=await db.Users.SingleOrDefaultAsync(x=>x.Email==r.Email.Trim().ToLowerInvariant(),ct);if(u is null)return;await SendPasswordResetOtpAsync(u,ct);}
    public async Task ResendPasswordResetOtpAsync(ForgotPasswordRequest r,CancellationToken ct)=>await ForgotPasswordAsync(r,ct);
    public async Task ResetPasswordAsync(ResetPasswordRequest r,CancellationToken ct){if(r.NewPassword.Length<8)throw new ArgumentException("Password must be at least 8 characters.");if(!IsOtp(r.Otp))throw new UnauthorizedException("Invalid reset request.");var u=await db.Users.SingleOrDefaultAsync(x=>x.Email==r.Email.Trim().ToLowerInvariant(),ct)??throw new UnauthorizedException("Invalid reset request.");var t=await db.PasswordResetTokens.SingleOrDefaultAsync(x=>x.UserId==u.Id&&x.TokenHash==HashToken(r.Otp)&&x.UsedAt==null&&x.ExpiresAt>DateTimeOffset.UtcNow,ct)??throw new UnauthorizedException("Invalid reset request.");u.PasswordHash=HashPassword(r.NewPassword);t.UsedAt=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);}
    public async Task ChangePasswordAsync(Guid id,ChangePasswordRequest r,CancellationToken ct){if(r.NewPassword.Length<8)throw new ArgumentException("Password must be at least 8 characters.");var u=await db.Users.SingleAsync(x=>x.Id==id,ct);if(!VerifyPassword(r.CurrentPassword,u.PasswordHash))throw new UnauthorizedException("Current password is incorrect.");u.PasswordHash=HashPassword(r.NewPassword);await db.SaveChangesAsync(ct);}
    private async Task SendVerificationAsync(User u,CancellationToken ct){var active=await db.EmailVerificationTokens.Where(x=>x.UserId==u.Id&&x.UsedAt==null).ToListAsync(ct);db.EmailVerificationTokens.RemoveRange(active);var otp=CreateOtp();db.EmailVerificationTokens.Add(new EmailVerificationToken{UserId=u.Id,TokenHash=HashToken(otp),ExpiresAt=DateTimeOffset.UtcNow.AddMinutes(10)});await db.SaveChangesAsync(ct);await email.SendAsync(u.Email,"Verify your email",$"Your verification OTP is: {otp}. It expires in 10 minutes.",ct);}
    private async Task SendPasswordResetOtpAsync(User u,CancellationToken ct){var active=await db.PasswordResetTokens.Where(x=>x.UserId==u.Id&&x.UsedAt==null).ToListAsync(ct);db.PasswordResetTokens.RemoveRange(active);var otp=CreateOtp();db.PasswordResetTokens.Add(new PasswordResetToken{UserId=u.Id,TokenHash=HashToken(otp),ExpiresAt=DateTimeOffset.UtcNow.AddMinutes(10)});await db.SaveChangesAsync(ct);await email.SendAsync(u.Email,"Reset your password",$"Your password reset OTP is: {otp}. It expires in 10 minutes.",ct);}

    private async Task<AuthResponse> CreateSessionAsync(User user, CancellationToken cancellationToken)
    {
        EnsureJwtKeyIsConfigured();
        var accessExpiry = DateTimeOffset.UtcNow.AddMinutes(int.TryParse(configuration["Jwt:AccessTokenExpirationMinutes"], out var accessMinutes) ? accessMinutes : 15);
        var refreshValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id, TokenHash = HashToken(refreshValue),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(int.TryParse(configuration["Jwt:RefreshTokenExpirationDays"], out var refreshDays) ? refreshDays : 30)
        });
        await db.SaveChangesAsync(cancellationToken);
        return new AuthResponse(CreateAccessToken(user, accessExpiry), refreshValue, accessExpiry, ToResponse(user));
    }

    private string CreateAccessToken(User user, DateTimeOffset expiresAt)
    {
        var key = configuration["Jwt:Key"]!;
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(JwtRegisteredClaimNames.Email, user.Email), new Claim("role", user.Role.ToString()) };
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims, expires: expiresAt.UtcDateTime, signingCredentials: credentials));
    }

    private static UserResponse ToResponse(User user) => new(user.Id, user.Name, user.Email, user.PhoneNumber, user.WhatsAppNumber, user.EmailVerified, user.Role.ToString());
    private void EnsureJwtKeyIsConfigured()
    {
        if (string.IsNullOrWhiteSpace(configuration["Jwt:Key"]) || configuration["Jwt:Key"]!.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be configured as a secret with at least 32 characters.");
    }
    private static string HashToken(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static string CreateOtp() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    private static bool IsOtp(string value) => value.Length == 6 && value.All(char.IsAsciiDigit);
    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 600_000, HashAlgorithmName.SHA512, 32);
        return $"v1.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }
    private static bool VerifyPassword(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 3 || parts[0] != "v1") return false;
        var expected = Convert.FromBase64String(parts[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(parts[1]), 600_000, HashAlgorithmName.SHA512, 32);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
