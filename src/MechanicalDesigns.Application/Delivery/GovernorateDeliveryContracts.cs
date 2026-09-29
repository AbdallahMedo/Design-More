namespace MechanicalDesigns.Application.Delivery;

public sealed record GovernorateDeliveryFeeResponse(Guid Id,string Name,string NameAr,decimal? DeliveryFee,bool IsActive);
public sealed record GovernorateDeliveryFeeUpdateRequest(decimal? DeliveryFee,bool IsActive);

public interface IGovernorateDeliveryFeeService
{
    Task<IReadOnlyList<GovernorateDeliveryFeeResponse>> GetPublicAsync(CancellationToken ct);
    Task<IReadOnlyList<GovernorateDeliveryFeeResponse>> GetAdminAsync(CancellationToken ct);
    Task<GovernorateDeliveryFeeResponse?> UpdateAsync(Guid id,GovernorateDeliveryFeeUpdateRequest request,CancellationToken ct);
}
