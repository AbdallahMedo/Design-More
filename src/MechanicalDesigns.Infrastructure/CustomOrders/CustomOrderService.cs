using MechanicalDesigns.Application.CustomOrders;
using MechanicalDesigns.Domain.Entities;
using MechanicalDesigns.Domain.Enums;
using MechanicalDesigns.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
namespace MechanicalDesigns.Infrastructure.CustomOrders;
public sealed class CustomOrderService(ApplicationDbContext db,IHttpContextAccessor httpContextAccessor):ICustomOrderService
{
 public async Task<CustomOrderResponse>CreateAsync(Guid? u,CustomOrderCreateRequest r,CancellationToken ct){var x=new CustomOrder{UserId=u,Name=r.Name.Trim(),Email=r.Email.Trim().ToLowerInvariant(),WhatsAppNumber=r.WhatsAppNumber.Trim(),Description=r.Description.Trim(),Notes=r.Notes?.Trim()};db.CustomOrders.Add(x);await db.SaveChangesAsync(ct);return Map(x);}
 public async Task<IReadOnlyList<CustomOrderResponse>>MineAsync(Guid u,CancellationToken ct)=>(await db.CustomOrders.AsNoTracking().Include(x=>x.Images).Where(x=>x.UserId==u).OrderByDescending(x=>x.CreatedAt).ToListAsync(ct)).Select(Map).ToList();
 public async Task<CustomOrderResponse?>GetMineAsync(Guid id,Guid u,CancellationToken ct){var x=await db.CustomOrders.AsNoTracking().Include(x=>x.Images).SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==u,ct);return x is null?null:Map(x);}
 public async Task<IReadOnlyList<CustomOrderResponse>>AdminAsync(string? status,CancellationToken ct){var q=db.CustomOrders.AsNoTracking().Include(x=>x.Images).AsQueryable();if(Enum.TryParse<CustomOrderStatus>(status,true,out var s))q=q.Where(x=>x.Status==s);return(await q.OrderByDescending(x=>x.CreatedAt).ToListAsync(ct)).Select(Map).ToList();}
 public async Task<bool>StatusAsync(Guid id,string status,CancellationToken ct){if(!Enum.TryParse<CustomOrderStatus>(status,true,out var s))throw new ArgumentException("Invalid custom order status.");var x=await db.CustomOrders.SingleOrDefaultAsync(x=>x.Id==id,ct);if(x is null)return false;x.Status=s;await db.SaveChangesAsync(ct);return true;}
 public async Task<IReadOnlyList<CustomOrderImageResponse>?>GetImagesAsync(Guid id,Guid? userId,bool admin,CancellationToken ct){var q=db.CustomOrders.AsNoTracking().Where(x=>x.Id==id);if(!admin)q=q.Where(x=>x.UserId==userId);if(!await q.AnyAsync(ct))return null;return(await db.CustomOrderImages.AsNoTracking().Where(x=>x.CustomOrderId==id).OrderBy(x=>x.DisplayOrder).ToListAsync(ct)).Select(MapImage).ToList();}
 public async Task<CustomOrderImageResponse?>AddImageAsync(Guid id,Guid userId,string url,int order,CancellationToken ct){var owner=await db.CustomOrders.SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==userId,ct);if(owner is null)return null;var count=await db.CustomOrderImages.CountAsync(x=>x.CustomOrderId==id,ct);if(count>=5)throw new ArgumentException("A custom order can contain at most 5 images.");var image=new CustomOrderImage{CustomOrderId=id,ImageUrl=url,DisplayOrder=Math.Max(0,order)};db.CustomOrderImages.Add(image);await db.SaveChangesAsync(ct);return MapImage(image);}
 public async Task<bool>DeleteImageAsync(Guid orderId,Guid imageId,Guid userId,bool admin,CancellationToken ct){var q=db.CustomOrderImages.Include(x=>x.CustomOrder).Where(x=>x.Id==imageId&&x.CustomOrderId==orderId);if(!admin)q=q.Where(x=>x.CustomOrder.UserId==userId);var image=await q.SingleOrDefaultAsync(ct);if(image is null)return false;db.CustomOrderImages.Remove(image);await db.SaveChangesAsync(ct);return true;}
 private CustomOrderResponse Map(CustomOrder x)=>new(x.Id,x.UserId,x.Name,x.Email,x.WhatsAppNumber,x.Description,x.Notes,x.Status.ToString(),x.CreatedAt,x.Images.OrderBy(i=>i.DisplayOrder).Select(MapImage).ToList());
 private CustomOrderImageResponse MapImage(CustomOrderImage image)=>new(image.Id,image.CustomOrderId,ToPublicUrl(image.ImageUrl),image.DisplayOrder,image.CreatedAt);
 private string ToPublicUrl(string url){if(!url.StartsWith("/",StringComparison.Ordinal)||Uri.IsWellFormedUriString(url,UriKind.Absolute))return url;var request=httpContextAccessor.HttpContext?.Request;return request is null?url:$"{request.Scheme}://{request.Host}{request.PathBase}{url}";}
}
