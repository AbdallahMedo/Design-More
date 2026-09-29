using MechanicalDesigns.Domain.Enums;

namespace MechanicalDesigns.Domain.Entities;

public sealed class Cart : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public sealed class CartItem : Entity
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public sealed class GovernorateDeliveryFee : Entity
{
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public decimal? DeliveryFee { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class Order : Entity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? WhatsAppNumber { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Governorate { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public OrderStatus OrderStatus { get; set; } = OrderStatus.PendingConfirmation;
    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    public User User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}

public sealed class OrderItem : Entity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductTitleAr { get; set; } = string.Empty;
    public string ProductTitleEn { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public OrderItemStatus Status { get; set; } = OrderItemStatus.Available;
    public Order Order { get; set; } = null!;
}

public sealed class OrderStatusHistory : Entity
{
    public Guid OrderId { get; set; }
    public OrderStatus? OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public string? Notes { get; set; }
    public Order Order { get; set; } = null!;
    public User? ChangedByUser { get; set; }
}
