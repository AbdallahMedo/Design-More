using MechanicalDesigns.Application.Common;
using MechanicalDesigns.Application.Reviews;
using MechanicalDesigns.Domain.Entities;
using MechanicalDesigns.Domain.Enums;
using MechanicalDesigns.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MechanicalDesigns.Infrastructure.Reviews;

public sealed class ReviewService(ApplicationDbContext db) : IReviewService
{
    public async Task<IReadOnlyList<ReviewResponse>> GetPublicAsync(Guid productId, CancellationToken ct) => await db.Reviews.AsNoTracking().Where(x => x.ProductId == productId && x.Status == ReviewStatus.Approved && x.Product.IsActive).OrderByDescending(x => x.CreatedAt).Select(ToResponse).ToListAsync(ct);
    public async Task<ReviewResponse> CreateAsync(Guid productId, Guid userId, ReviewCreateRequest r, CancellationToken ct)
    {
        Validate(r.Rating); if (!await db.Products.AnyAsync(x => x.Id == productId && x.IsActive, ct)) throw new ConflictException("Product not found or inactive.");
        if (await db.Reviews.AnyAsync(x => x.ProductId == productId && x.UserId == userId, ct)) throw new ConflictException("Only one review per product is allowed.");
        var review = new Review { ProductId = productId, UserId = userId, Rating = r.Rating, Comment = r.Comment?.Trim(), Status = ReviewStatus.Pending }; db.Reviews.Add(review); await db.SaveChangesAsync(ct);
        return await db.Reviews.AsNoTracking().Where(x => x.Id == review.Id).Select(ToResponse).SingleAsync(ct);
    }
    public async Task<ReviewResponse?> UpdateAsync(Guid productId, Guid reviewId, Guid userId, ReviewUpdateRequest r, CancellationToken ct)
    {
        Validate(r.Rating); var review = await db.Reviews.SingleOrDefaultAsync(x => x.Id == reviewId && x.ProductId == productId && x.UserId == userId, ct); if (review is null) return null;
        review.Rating = r.Rating; review.Comment = r.Comment?.Trim(); review.Status = ReviewStatus.Pending; await db.SaveChangesAsync(ct);
        return await db.Reviews.AsNoTracking().Where(x => x.Id == reviewId).Select(ToResponse).SingleAsync(ct);
    }
    public async Task<bool> DeleteAsync(Guid productId, Guid reviewId, Guid userId, bool isAdmin, CancellationToken ct)
    {
        var query = db.Reviews.Where(x => x.Id == reviewId); if (productId != Guid.Empty) query = query.Where(x => x.ProductId == productId); if (!isAdmin) query = query.Where(x => x.UserId == userId);
        var review = await query.SingleOrDefaultAsync(ct); if (review is null) return false; db.Reviews.Remove(review); await db.SaveChangesAsync(ct); return true;
    }
    public async Task<IReadOnlyList<ReviewResponse>> GetAdminAsync(ReviewQuery q, CancellationToken ct)
    {
        var query = db.Reviews.AsNoTracking().AsQueryable(); if (q.ProductId is not null) query = query.Where(x => x.ProductId == q.ProductId); if (Enum.TryParse<ReviewStatus>(q.Status, true, out var status)) query = query.Where(x => x.Status == status);
        return await query.OrderByDescending(x => x.CreatedAt).Skip((Math.Max(1,q.Page)-1)*Math.Clamp(q.PageSize,1,100)).Take(Math.Clamp(q.PageSize,1,100)).Select(ToResponse).ToListAsync(ct);
    }
    public async Task<bool> SetStatusAsync(Guid id, string status, CancellationToken ct) { if (!Enum.TryParse<ReviewStatus>(status, true, out var parsed) || parsed == ReviewStatus.Pending) throw new ArgumentException("Status must be Approved or Rejected."); var review=await db.Reviews.SingleOrDefaultAsync(x=>x.Id==id,ct); if(review is null)return false; review.Status=parsed; await db.SaveChangesAsync(ct); return true; }
    private static void Validate(int rating) { if (rating is < 1 or > 5) throw new ArgumentException("Rating must be between 1 and 5."); }
    private static readonly System.Linq.Expressions.Expression<Func<Review,ReviewResponse>> ToResponse=x=>new(x.Id,x.ProductId,x.UserId,x.User.Name,x.Rating,x.Comment,x.Status.ToString(),x.CreatedAt,x.UpdatedAt);
}
