namespace MechanicalDesigns.Application.Reviews;

public sealed record ReviewResponse(Guid Id, Guid ProductId, Guid UserId, string UserName, int Rating, string? Comment, string Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
public sealed record ReviewCreateRequest(int Rating, string? Comment);
public sealed record ReviewUpdateRequest(int Rating, string? Comment);
public sealed record ReviewQuery(Guid? ProductId, string? Status, int Page = 1, int PageSize = 20);

public interface IReviewService
{
    Task<IReadOnlyList<ReviewResponse>> GetPublicAsync(Guid productId, CancellationToken ct);
    Task<ReviewResponse> CreateAsync(Guid productId, Guid userId, ReviewCreateRequest request, CancellationToken ct);
    Task<ReviewResponse?> UpdateAsync(Guid productId, Guid reviewId, Guid userId, ReviewUpdateRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid productId, Guid reviewId, Guid userId, bool isAdmin, CancellationToken ct);
    Task<IReadOnlyList<ReviewResponse>> GetAdminAsync(ReviewQuery query, CancellationToken ct);
    Task<bool> SetStatusAsync(Guid id, string status, CancellationToken ct);
}
