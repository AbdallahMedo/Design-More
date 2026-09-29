namespace MechanicalDesigns.Application.Products;

public sealed record ProductImageResponse(Guid Id, Guid ProductId, string ImageUrl, int DisplayOrder, bool IsPrimary, DateTimeOffset CreatedAt);
public sealed record ProductImageCreateRequest(string ImageUrl, int DisplayOrder = 0, bool IsPrimary = false);

public interface IProductImageService
{
    Task<IReadOnlyList<ProductImageResponse>> GetAsync(Guid productId, bool publicOnly, CancellationToken cancellationToken);
    Task<ProductImageResponse?> CreateAsync(Guid productId, ProductImageCreateRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid productId, Guid imageId, CancellationToken cancellationToken);
}
