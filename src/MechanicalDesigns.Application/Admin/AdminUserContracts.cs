namespace MechanicalDesigns.Application.Admin;
public sealed record AdminUserResponse(Guid Id,string Name,string Email,string? PhoneNumber,string? WhatsAppNumber,bool EmailVerified,bool IsBlocked,string Role,DateTimeOffset CreatedAt);
public sealed record AdminCreateRequest(string Name,string Email,string Password,string? PhoneNumber,string? WhatsAppNumber);
public interface IAdminUserService{Task<IReadOnlyList<AdminUserResponse>>ListAsync(string? search,CancellationToken ct);Task<AdminUserResponse?>GetAsync(Guid id,CancellationToken ct);Task<AdminUserResponse>CreateAdminAsync(AdminCreateRequest request,CancellationToken ct);Task<bool>BlockAsync(Guid id,bool blocked,CancellationToken ct);}
