namespace MechanicalDesigns.Application.CustomOrders;
public sealed record CustomOrderCreateRequest(string Name,string Email,string WhatsAppNumber,string Description,string? Notes);
public sealed record CustomOrderStatusRequest(string Status);
public sealed record CustomOrderImageResponse(Guid Id,Guid CustomOrderId,string ImageUrl,int DisplayOrder,DateTimeOffset CreatedAt)
{
    public string Url => ImageUrl;
}
public sealed record CustomOrderClientResponse(string Name,string Email,string Phone)
{
    public string FullName => Name;
    public string PhoneNumber => Phone;
    public string WhatsAppNumber => Phone;
}
public sealed record CustomOrderResponse(Guid Id,Guid? UserId,string Name,string? Email,string WhatsAppNumber,string Description,string? Notes,string Status,DateTimeOffset CreatedAt,IReadOnlyList<CustomOrderImageResponse> Images)
{
    // Dashboard-friendly aliases. Existing data stores the enquiry text as Description
    // and contact details as Name/WhatsAppNumber.
    public string Title => Description;
    public string ClientName => Name;
    public string ClientEmail => string.IsNullOrWhiteSpace(Email) ? "N/A" : Email;
    public string ClientPhone => WhatsAppNumber;
    public string CustomerName => ClientName;
    public string CustomerEmail => ClientEmail;
    public string CustomerPhone => ClientPhone;
    public CustomOrderClientResponse Client => new(ClientName, ClientEmail, ClientPhone);
    public CustomOrderClientResponse Customer => Client;
    public IReadOnlyList<CustomOrderImageResponse> ReferenceImages => Images;
    public IReadOnlyList<CustomOrderImageResponse> Attachments => Images;
    public IReadOnlyList<string> ImageUrls => Images.Select(x => x.ImageUrl).ToList();
};
public interface ICustomOrderService{Task<CustomOrderResponse>CreateAsync(Guid? userId,CustomOrderCreateRequest r,CancellationToken ct);Task<IReadOnlyList<CustomOrderResponse>>MineAsync(Guid userId,CancellationToken ct);Task<CustomOrderResponse?>GetMineAsync(Guid orderId,Guid userId,CancellationToken ct);Task<IReadOnlyList<CustomOrderResponse>>AdminAsync(string? status,CancellationToken ct);Task<bool>StatusAsync(Guid id,string status,CancellationToken ct);Task<IReadOnlyList<CustomOrderImageResponse>?>GetImagesAsync(Guid orderId,Guid? userId,bool isAdmin,CancellationToken ct);Task<CustomOrderImageResponse?>AddImageAsync(Guid orderId,Guid userId,string imageUrl,int displayOrder,CancellationToken ct);Task<bool>DeleteImageAsync(Guid orderId,Guid imageId,Guid userId,bool isAdmin,CancellationToken ct);}
