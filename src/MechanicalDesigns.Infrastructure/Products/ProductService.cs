using MechanicalDesigns.Application.Common;
using MechanicalDesigns.Application.Products;
using MechanicalDesigns.Domain.Entities;
using MechanicalDesigns.Domain.Enums;
using MechanicalDesigns.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MechanicalDesigns.Infrastructure.Products;

public sealed class ProductService(ApplicationDbContext db) : IProductService
{
    public Task<PagedResponse<ProductResponse>> GetPublicAsync(ProductQuery query, CancellationToken ct) => GetAsync(db.Products.AsNoTracking().Where(x => x.IsActive && x.Category.IsActive), query, ct);
    public Task<ProductResponse?> GetPublicAsync(Guid id, CancellationToken ct) => PublicBase().Where(x => x.Id == id).Select(ToResponse).SingleOrDefaultAsync(ct);
    public Task<ProductResponse?> GetPublicBySlugAsync(string slug, CancellationToken ct) => PublicBase().Where(x => x.Slug == slug.Trim().ToLowerInvariant()).Select(ToResponse).SingleOrDefaultAsync(ct);
    public Task<PagedResponse<ProductResponse>> GetAdminAsync(ProductQuery query, CancellationToken ct) => GetAsync(db.Products.AsNoTracking(), query, ct);

    public Task<ProductResponse?> GetAdminAsync(Guid id, CancellationToken ct) => db.Products.AsNoTracking().Where(x => x.Id == id).Select(ToResponse).SingleOrDefaultAsync(ct);

    public async Task<ProductResponse> CreateAsync(ProductUpsertRequest request, CancellationToken ct)
    {
        await ValidateAsync(request, null, ct);
        var product = Apply(new Product(), request); db.Products.Add(product); await db.SaveChangesAsync(ct); return await GetAdminByIdAsync(product.Id, ct);
    }
    public async Task<ProductResponse?> UpdateAsync(Guid id, ProductUpsertRequest request, CancellationToken ct)
    {
        var product = await db.Products.SingleOrDefaultAsync(x => x.Id == id, ct); if (product is null) return null;
        await ValidateAsync(request, id, ct); Apply(product, request); await db.SaveChangesAsync(ct); return await GetAdminByIdAsync(id, ct);
    }
    public async Task<bool> SetStatusAsync(Guid id, bool isActive, CancellationToken ct) { var p = await db.Products.SingleOrDefaultAsync(x => x.Id == id, ct); if (p is null) return false; p.IsActive = isActive; await db.SaveChangesAsync(ct); return true; }
    public async Task<bool> SetStockAsync(Guid id, int stock, CancellationToken ct) { if (stock < 0) throw new ArgumentOutOfRangeException(nameof(stock)); var p = await db.Products.SingleOrDefaultAsync(x => x.Id == id, ct); if (p is null) return false; p.StockQuantity = stock; await db.SaveChangesAsync(ct); return true; }
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) { var p = await db.Products.SingleOrDefaultAsync(x => x.Id == id, ct); if (p is null) return false; p.IsDeleted = true; p.DeletedAt = DateTimeOffset.UtcNow; p.IsActive = false; await db.SaveChangesAsync(ct); return true; }

    private IQueryable<Product> PublicBase() => db.Products.AsNoTracking().Where(x => x.IsActive && x.Category.IsActive);
    private async Task<PagedResponse<ProductResponse>> GetAsync(IQueryable<Product> source, ProductQuery q, CancellationToken ct)
    {
        var page = Math.Max(1, q.Page); var size = Math.Clamp(q.PageSize, 1, 100);
        if (q.CategoryId is not null) source = source.Where(x => x.CategoryId == q.CategoryId);
        if (!string.IsNullOrWhiteSpace(q.Search)) { var s = q.Search.Trim(); source = source.Where(x => EF.Functions.ILike(x.TitleAr, $"%{s}%") || EF.Functions.ILike(x.TitleEn, $"%{s}%") || EF.Functions.ILike(x.DescriptionAr!, $"%{s}%") || EF.Functions.ILike(x.DescriptionEn!, $"%{s}%")); }
        if (q.MinPrice is not null) source = source.Where(x => x.Price >= q.MinPrice); if (q.MaxPrice is not null) source = source.Where(x => x.Price <= q.MaxPrice); if (q.InStock is not null) source = q.InStock.Value ? source.Where(x => x.StockQuantity > 0) : source.Where(x => x.StockQuantity == 0);
        if (q.Rating is not null) source = source.Where(x => x.Reviews.Where(r => r.Status == ReviewStatus.Approved).Select(r => (decimal?)r.Rating).Average() >= q.Rating);
        source = q.SortBy?.ToLowerInvariant() switch { "oldest" => source.OrderBy(x => x.CreatedAt), "pricelowtohigh" => source.OrderBy(x => x.Price), "pricehightolow" => source.OrderByDescending(x => x.Price), "highestrated" => source.OrderByDescending(x => x.Reviews.Where(r => r.Status == ReviewStatus.Approved).Select(r => (decimal?)r.Rating).Average()), _ => source.OrderByDescending(x => x.CreatedAt) };
        var total = await source.CountAsync(ct); var items = await source.Skip((page - 1) * size).Take(size).Select(ToResponse).ToListAsync(ct);
        return new PagedResponse<ProductResponse>(items, page, size, total, (int)Math.Ceiling(total / (double)size));
    }
    private async Task ValidateAsync(ProductUpsertRequest r, Guid? currentId, CancellationToken ct)
    {
        if (!await db.Categories.AnyAsync(x => x.Id == r.CategoryId && x.IsActive, ct)) throw new ConflictException("The category does not exist or is inactive.");
        var slug = r.Slug.Trim().ToLowerInvariant(); if (await db.Products.IgnoreQueryFilters().AnyAsync(x => x.Slug == slug && x.Id != currentId, ct)) throw new ConflictException("A product with this slug already exists.");
    }
    private static Product Apply(Product p, ProductUpsertRequest r) { p.CategoryId = r.CategoryId; p.TitleAr = r.TitleAr.Trim(); p.TitleEn = r.TitleEn.Trim(); p.SubtitleAr = r.SubtitleAr?.Trim(); p.SubtitleEn = r.SubtitleEn?.Trim(); p.DescriptionAr = r.DescriptionAr?.Trim(); p.DescriptionEn = r.DescriptionEn?.Trim(); p.Price = r.Price; p.StockQuantity = r.StockQuantity; p.Slug = r.Slug.Trim().ToLowerInvariant(); return p; }
    private Task<ProductResponse> GetAdminByIdAsync(Guid id, CancellationToken ct) => db.Products.IgnoreQueryFilters().AsNoTracking().Where(x => x.Id == id).Select(ToResponse).SingleAsync(ct);
    private static readonly System.Linq.Expressions.Expression<Func<Product, ProductResponse>> ToResponse = x => new(x.Id, x.CategoryId, x.TitleAr, x.TitleEn, x.SubtitleAr, x.SubtitleEn, x.DescriptionAr, x.DescriptionEn, x.Price, x.StockQuantity, x.StockQuantity > 0, x.IsActive, x.Slug, x.Reviews.Where(r => r.Status == ReviewStatus.Approved).Select(r => (decimal?)r.Rating).Average() ?? 0, x.Reviews.Count(r => r.Status == ReviewStatus.Approved), x.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).Select(i => new ProductImageResponse(i.Id, i.ProductId, i.ImageUrl, i.DisplayOrder, i.IsPrimary, i.CreatedAt)).ToList(), x.CreatedAt, x.UpdatedAt);
}
