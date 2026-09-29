using MechanicalDesigns.Application.Delivery;
using MechanicalDesigns.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MechanicalDesigns.Infrastructure.Delivery;

public sealed class GovernorateDeliveryFeeService(ApplicationDbContext db) : IGovernorateDeliveryFeeService
{
    public async Task<IReadOnlyList<GovernorateDeliveryFeeResponse>> GetPublicAsync(CancellationToken ct) =>
        await db.GovernorateDeliveryFees.AsNoTracking().Where(x => x.IsActive && x.DeliveryFee != null).OrderBy(x => x.Name).Select(x => Map(x)).ToListAsync(ct);

    public async Task<IReadOnlyList<GovernorateDeliveryFeeResponse>> GetAdminAsync(CancellationToken ct) =>
        await db.GovernorateDeliveryFees.AsNoTracking().OrderBy(x => x.Name).Select(x => Map(x)).ToListAsync(ct);

    public async Task<GovernorateDeliveryFeeResponse?> UpdateAsync(Guid id,GovernorateDeliveryFeeUpdateRequest request,CancellationToken ct)
    {
        if (request.DeliveryFee is < 0) throw new ArgumentException("Delivery fee cannot be negative.");
        var governorate = await db.GovernorateDeliveryFees.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (governorate is null) return null;
        governorate.DeliveryFee = request.DeliveryFee;
        governorate.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return Map(governorate);
    }

    private static GovernorateDeliveryFeeResponse Map(MechanicalDesigns.Domain.Entities.GovernorateDeliveryFee x) => new(x.Id,x.Name,x.NameAr,x.DeliveryFee,x.IsActive);
}
