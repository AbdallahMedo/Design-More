using System.Security.Cryptography;
using MechanicalDesigns.Domain.Entities;
using MechanicalDesigns.Domain.Enums;
using MechanicalDesigns.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MechanicalDesigns.Infrastructure.Authentication;

public sealed class AdminBootstrapper(ApplicationDbContext db, IConfiguration configuration)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var email = configuration["SeedAdmin:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["SeedAdmin:Password"];

        // Bootstrap is opt-in: empty committed settings must never create an account.
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        if (password.Length < 12)
            throw new InvalidOperationException("SeedAdmin:Password must contain at least 12 characters.");

        var existing = await db.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (existing is null)
        {
            db.Users.Add(new User
            {
                Name = "Administrator",
                Email = email,
                PasswordHash = HashPassword(password),
                EmailVerified = true,
                Role = UserRole.Admin
            });
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        // The configured email is always promoted, but a configured password never
        // overwrites an existing account password on future application starts.
        if (existing.Role != UserRole.Admin)
        {
            existing.Role = UserRole.Admin;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 600_000, HashAlgorithmName.SHA512, 32);
        return $"v1.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }
}
