using MechanicalDesigns.Domain.Enums;

namespace MechanicalDesigns.Domain.Entities;

public sealed class CustomOrder : Entity
{
    public Guid? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string WhatsAppNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public CustomOrderStatus Status { get; set; } = CustomOrderStatus.New;
    public User? User { get; set; }
    public ICollection<CustomOrderImage> Images { get; set; } = new List<CustomOrderImage>();
}

public sealed class CustomOrderImage : Entity
{
    public Guid CustomOrderId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public CustomOrder CustomOrder { get; set; } = null!;
}

public sealed class Advertisement : SoftDeletableEntity
{
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? MobileImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AuditLog : Entity
{
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? Metadata { get; set; }
    public User? User { get; set; }
}
