using MechanicalDesigns.Application.Cart;
using MechanicalDesigns.Application.Common;
using MechanicalDesigns.Domain.Entities;
using MechanicalDesigns.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MechanicalDesigns.Infrastructure.ShoppingCarts;
public sealed class CartService(ApplicationDbContext db):ICartService
{
 public async Task<CartResponse> GetAsync(Guid u,CancellationToken ct)=>Map(await GetOrCreate(u,ct));
 public async Task<CartResponse> AddAsync(Guid u,CartItemCreateRequest r,CancellationToken ct){if(r.Quantity<=0)throw new ArgumentException("Quantity must be greater than zero.");var p=await db.Products.SingleOrDefaultAsync(x=>x.Id==r.ProductId&&x.IsActive,ct)??throw new ConflictException("Product not found or inactive.");var c=await GetOrCreate(u,ct);var i=c.Items.SingleOrDefault(x=>x.ProductId==r.ProductId);var qty=(i?.Quantity??0)+r.Quantity;if(qty>p.StockQuantity)throw new ConflictException("Requested quantity exceeds stock.");if(i is null){i=new CartItem{CartId=c.Id,ProductId=p.Id,Quantity=r.Quantity,Product=p};db.CartItems.Add(i);c.Items.Add(i);}else i.Quantity=qty;try{await db.SaveChangesAsync(ct);}catch(DbUpdateConcurrencyException){throw new ConflictException("Your cart changed concurrently. Reload it and try again.");}return Map(c);}
 public async Task<CartResponse?> UpdateAsync(Guid u,Guid itemId,CartItemUpdateRequest r,CancellationToken ct){if(r.Quantity<=0)throw new ArgumentException("Quantity must be greater than zero.");var c=await GetOrCreate(u,ct);var i=c.Items.SingleOrDefault(x=>x.Id==itemId);if(i is null)return null;if(!i.Product.IsActive||r.Quantity>i.Product.StockQuantity)throw new ConflictException("Product is unavailable or stock is insufficient.");i.Quantity=r.Quantity;await db.SaveChangesAsync(ct);return Map(c);}
 public async Task<bool> RemoveAsync(Guid u,Guid itemId,CancellationToken ct){var c=await GetOrCreate(u,ct);var i=c.Items.SingleOrDefault(x=>x.Id==itemId);if(i is null)return false;db.CartItems.Remove(i);await db.SaveChangesAsync(ct);return true;}
 public async Task ClearAsync(Guid u,CancellationToken ct){var c=await GetOrCreate(u,ct);db.CartItems.RemoveRange(c.Items);await db.SaveChangesAsync(ct);}
 private async Task<Cart> GetOrCreate(Guid u,CancellationToken ct){var c=await db.Carts.Include(x=>x.Items).ThenInclude(x=>x.Product).SingleOrDefaultAsync(x=>x.UserId==u,ct);if(c is not null)return c;c=new Cart{UserId=u};db.Carts.Add(c);await db.SaveChangesAsync(ct);return c;}
 private static CartResponse Map(Cart c){var i=c.Items.OrderBy(x=>x.CreatedAt).Select(x=>new CartItemResponse(x.Id,x.ProductId,x.Product.TitleAr,x.Product.TitleEn,x.Product.Price,x.Quantity,x.Product.StockQuantity,x.Product.Price*x.Quantity)).ToList();return new(c.Id,i,i.Sum(x=>x.LineTotal),c.UpdatedAt);}
}
