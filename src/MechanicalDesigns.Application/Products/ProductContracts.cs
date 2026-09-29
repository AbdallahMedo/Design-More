namespace MechanicalDesigns.Application.Products;

public sealed record ProductResponse(Guid Id, Guid CategoryId, string TitleAr, string TitleEn, string? SubtitleAr, string? SubtitleEn, string? DescriptionAr, string? DescriptionEn, decimal Price, int StockQuantity, bool IsInStock, bool IsActive, string Slug, decimal AverageRating, int ReviewsCount, IReadOnlyList<ProductImageResponse> Images, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
public sealed record ProductUpsertRequest(Guid CategoryId, string TitleAr, string TitleEn, string? SubtitleAr, string? SubtitleEn, string? DescriptionAr, string? DescriptionEn, decimal Price, int StockQuantity, string Slug);
public sealed record ProductStatusRequest(bool IsActive);
public sealed record ProductStockRequest(int StockQuantity);
public sealed record ProductQuery(Guid? CategoryId, string? Search, decimal? MinPrice, decimal? MaxPrice, bool? InStock, decimal? Rating, string? SortBy, int Page = 1, int PageSize = 20);
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems, int TotalPages);

public interface IProductService
{
    Task<PagedResponse<ProductResponse>> GetPublicAsync(ProductQuery query, CancellationToken cancellationToken);
    Task<ProductResponse?> GetPublicAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductResponse?> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<PagedResponse<ProductResponse>> GetAdminAsync(ProductQuery query, CancellationToken cancellationToken);
    Task<ProductResponse?> GetAdminAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductResponse> CreateAsync(ProductUpsertRequest request, CancellationToken cancellationToken);
    Task<ProductResponse?> UpdateAsync(Guid id, ProductUpsertRequest request, CancellationToken cancellationToken);
    Task<bool> SetStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken);
    Task<bool> SetStockAsync(Guid id, int stockQuantity, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
