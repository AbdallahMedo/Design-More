using MechanicalDesigns.Application.Products;
using MechanicalDesigns.Domain.Entities;
using MechanicalDesigns.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MechanicalDesigns.Infrastructure.Products;

public sealed class ProductImageService(ApplicationDbContext db) : IProductImageService
{
    public async Task<IReadOnlyList<ProductImageResponse>> GetAsync(Guid productId, bool publicOnly, CancellationToken ct)
    {
        var products = publicOnly ? db.Products.Where(x => x.IsActive && x.Category.IsActive) : db.Products.IgnoreQueryFilters();
        if (!await products.AnyAsync(x => x.Id == productId, ct)) return Array.Empty<ProductImageResponse>();
        return await db.ProductImages.AsNoTracking().Where(x => x.ProductId == productId).OrderByDescending(x => x.IsPrimary).ThenBy(x => x.DisplayOrder).Select(ToResponse).ToListAsync(ct);
    }

    public async Task<ProductImageResponse?> CreateAsync(Guid productId, ProductImageCreateRequest request, CancellationToken ct)
    {
        if (!Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            throw new ArgumentException("ImageUrl must be an absolute HTTP or HTTPS URL.");
        var product = await db.Products.SingleOrDefaultAsync(x => x.Id == productId, ct);
        if (product is null) return null;
        if (request.IsPrimary)
        {
            var primaryImages = await db.ProductImages.Where(x => x.ProductId == productId && x.IsPrimary).ToListAsync(ct);
            foreach (var existing in primaryImages) existing.IsPrimary = false;
        }
        var image = new ProductImage { ProductId = productId, ImageUrl = uri.ToString(), DisplayOrder = Math.Max(0, request.DisplayOrder), IsPrimary = request.IsPrimary };
        db.ProductImages.Add(image);
        await db.SaveChangesAsync(ct);
        return Map(image);
    }

    public async Task<bool> DeleteAsync(Guid productId, Guid imageId, CancellationToken ct)
    {
        var image = await db.ProductImages.SingleOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId, ct);
        if (image is null) return false;
        db.ProductImages.Remove(image);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static readonly System.Linq.Expressions.Expression<Func<ProductImage, ProductImageResponse>> ToResponse = x => new(x.Id, x.ProductId, x.ImageUrl, x.DisplayOrder, x.IsPrimary, x.CreatedAt);
    private static ProductImageResponse Map(ProductImage x) => new(x.Id, x.ProductId, x.ImageUrl, x.DisplayOrder, x.IsPrimary, x.CreatedAt);
}
