using MechanicalDesigns.Application.Categories;
using MechanicalDesigns.Application.Common;
using MechanicalDesigns.Domain.Entities;
using MechanicalDesigns.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MechanicalDesigns.Infrastructure.Categories;

public sealed class CategoryService(ApplicationDbContext db) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> GetPublicAsync(CancellationToken cancellationToken) =>
        await db.Categories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.NameEn).Select(ToResponse).ToListAsync(cancellationToken);

    public async Task<CategoryResponse?> GetPublicAsync(string idOrSlug, CancellationToken cancellationToken)
    {
        var slug = idOrSlug.Trim().ToLowerInvariant();
        var query = db.Categories.AsNoTracking().Where(x => x.IsActive);
        return Guid.TryParse(idOrSlug, out var id)
            ? await query.Where(x => x.Id == id).Select(ToResponse).SingleOrDefaultAsync(cancellationToken)
            : await query.Where(x => x.Slug == slug).Select(ToResponse).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAdminAsync(CancellationToken cancellationToken) =>
        await db.Categories.IgnoreQueryFilters().AsNoTracking().OrderBy(x => x.NameEn).Select(ToResponse).ToListAsync(cancellationToken);

    public async Task<CategoryResponse> CreateAsync(CategoryUpsertRequest request, CancellationToken cancellationToken)
    {
        var slug = NormalizeSlug(request.Slug);
        if (await db.Categories.IgnoreQueryFilters().AnyAsync(x => x.Slug == slug, cancellationToken))
            throw new ConflictException("A category with this slug already exists.");
        var category = new Category { NameAr = request.NameAr.Trim(), NameEn = request.NameEn.Trim(), Slug = slug, ImageUrl = request.ImageUrl?.Trim() };
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task<CategoryResponse?> UpdateAsync(Guid id, CategoryUpsertRequest request, CancellationToken cancellationToken)
    {
        var category = await db.Categories.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null) return null;
        var slug = NormalizeSlug(request.Slug);
        if (await db.Categories.IgnoreQueryFilters().AnyAsync(x => x.Slug == slug && x.Id != id, cancellationToken))
            throw new ConflictException("A category with this slug already exists.");
        category.NameAr = request.NameAr.Trim(); category.NameEn = request.NameEn.Trim(); category.Slug = slug; category.ImageUrl = request.ImageUrl?.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task<bool> SetStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var category = await db.Categories.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null) return false;
        category.IsActive = isActive;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await db.Categories.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null) return false;
        category.IsDeleted = true; category.DeletedAt = DateTimeOffset.UtcNow; category.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string NormalizeSlug(string slug) => slug.Trim().ToLowerInvariant();
    private static readonly System.Linq.Expressions.Expression<Func<Category, CategoryResponse>> ToResponse = x => new CategoryResponse(x.Id, x.NameAr, x.NameEn, x.Slug, x.ImageUrl, x.IsActive, x.IsDeleted, x.CreatedAt, x.UpdatedAt);
    private static CategoryResponse Map(Category x) => new(x.Id, x.NameAr, x.NameEn, x.Slug, x.ImageUrl, x.IsActive, x.IsDeleted, x.CreatedAt, x.UpdatedAt);
}
