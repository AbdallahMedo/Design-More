namespace MechanicalDesigns.Application.Cart;
public sealed record CartItemResponse(Guid Id,Guid ProductId,string TitleAr,string TitleEn,decimal Price,int Quantity,int StockQuantity,decimal LineTotal);
public sealed record CartResponse(Guid Id,IReadOnlyList<CartItemResponse> Items,decimal Subtotal,DateTimeOffset UpdatedAt);
public sealed record CartItemCreateRequest(Guid ProductId,int Quantity);
public sealed record CartItemUpdateRequest(int Quantity);
public interface ICartService { Task<CartResponse> GetAsync(Guid userId,CancellationToken ct); Task<CartResponse> AddAsync(Guid userId,CartItemCreateRequest r,CancellationToken ct); Task<CartResponse?> UpdateAsync(Guid userId,Guid itemId,CartItemUpdateRequest r,CancellationToken ct); Task<bool> RemoveAsync(Guid userId,Guid itemId,CancellationToken ct); Task ClearAsync(Guid userId,CancellationToken ct); }
