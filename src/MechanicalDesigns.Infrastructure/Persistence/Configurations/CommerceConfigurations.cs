using MechanicalDesigns.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicalDesigns.Infrastructure.Persistence.Configurations;

internal sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        UserConfiguration.ConfigureEntity(builder);
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems", table => table.HasCheckConstraint("CK_CartItems_Quantity", "\"Quantity\" > 0"));
        UserConfiguration.ConfigureEntity(builder);
        builder.HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();
        builder.HasOne(x => x.Cart).WithMany(x => x.Items).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Product).WithMany(x => x.CartItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.Product.IsDeleted);
    }
}

internal sealed class GovernorateDeliveryFeeConfiguration : IEntityTypeConfiguration<GovernorateDeliveryFee>
{
    public void Configure(EntityTypeBuilder<GovernorateDeliveryFee> builder)
    {
        builder.ToTable("GovernorateDeliveryFees", table => table.HasCheckConstraint("CK_GovernorateDeliveryFees_Fee", "\"DeliveryFee\" IS NULL OR \"DeliveryFee\" >= 0"));
        UserConfiguration.ConfigureEntity(builder);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DeliveryFee).HasPrecision(12, 2);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", table =>
        {
            table.HasCheckConstraint("CK_Orders_Subtotal", "\"Subtotal\" >= 0");
            table.HasCheckConstraint("CK_Orders_ShippingCost", "\"ShippingCost\" >= 0");
            table.HasCheckConstraint("CK_Orders_Total", "\"Total\" >= 0");
        });
        UserConfiguration.ConfigureEntity(builder);
        builder.Property(x => x.OrderNumber).HasMaxLength(40).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.WhatsAppNumber).HasMaxLength(30);
        builder.Property(x => x.Address).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.City).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Governorate).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Subtotal).HasPrecision(12, 2);
        builder.Property(x => x.ShippingCost).HasPrecision(12, 2);
        builder.Property(x => x.Total).HasPrecision(12, 2);
        builder.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.PaymentStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.OrderStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.CustomerNotes).HasMaxLength(2000);
        builder.Property(x => x.AdminNotes).HasMaxLength(2000);
        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
        builder.HasIndex(x => new { x.OrderStatus, x.CreatedAt });
        builder.HasOne(x => x.User).WithMany(x => x.Orders).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", table =>
        {
            table.HasCheckConstraint("CK_OrderItems_Quantity", "\"Quantity\" > 0");
            table.HasCheckConstraint("CK_OrderItems_UnitPrice", "\"UnitPrice\" >= 0");
            table.HasCheckConstraint("CK_OrderItems_TotalPrice", "\"TotalPrice\" >= 0");
        });
        UserConfiguration.ConfigureEntity(builder);
        builder.Property(x => x.ProductTitleAr).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ProductTitleEn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(12, 2);
        builder.Property(x => x.TotalPrice).HasPrecision(12, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.OrderId);
        builder.HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistories");
        UserConfiguration.ConfigureEntity(builder);
        builder.Property(x => x.OldStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.NewStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.HasIndex(x => new { x.OrderId, x.CreatedAt });
        builder.HasOne(x => x.Order).WithMany(x => x.StatusHistory).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
