namespace MechanicalDesigns.Application.Categories;

public sealed record CategoryResponse(Guid Id, string NameAr, string NameEn, string Slug, string? ImageUrl, bool IsActive, bool IsDeleted, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
public sealed record CategoryUpsertRequest(string NameAr, string NameEn, string Slug, string? ImageUrl);
public sealed record CategoryStatusRequest(bool IsActive);

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetPublicAsync(CancellationToken cancellationToken);
    Task<CategoryResponse?> GetPublicAsync(string idOrSlug, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryResponse>> GetAdminAsync(CancellationToken cancellationToken);
    Task<CategoryResponse> CreateAsync(CategoryUpsertRequest request, CancellationToken cancellationToken);
    Task<CategoryResponse?> UpdateAsync(Guid id, CategoryUpsertRequest request, CancellationToken cancellationToken);
    Task<bool> SetStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
