using MechanicalDesigns.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicalDesigns.Infrastructure.Persistence.Configurations;

internal sealed class CustomOrderConfiguration : IEntityTypeConfiguration<CustomOrder>
{
    public void Configure(EntityTypeBuilder<CustomOrder> builder)
    {
        builder.ToTable("CustomOrders");
        UserConfiguration.ConfigureEntity(builder);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320);
        builder.Property(x => x.WhatsAppNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(5000).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
        builder.HasOne(x => x.User).WithMany(x => x.CustomOrders).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class CustomOrderImageConfiguration : IEntityTypeConfiguration<CustomOrderImage>
{
    public void Configure(EntityTypeBuilder<CustomOrderImage> builder)
    {
        builder.ToTable("CustomOrderImages");
        UserConfiguration.ConfigureEntity(builder);
        builder.Property(x => x.ImageUrl).HasMaxLength(2048).IsRequired();
        builder.HasIndex(x => new { x.CustomOrderId, x.DisplayOrder });
        builder.HasOne(x => x.CustomOrder).WithMany(x => x.Images).HasForeignKey(x => x.CustomOrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class AdvertisementConfiguration : IEntityTypeConfiguration<Advertisement>
{
    public void Configure(EntityTypeBuilder<Advertisement> builder)
    {
        builder.ToTable("Advertisements", table => table.HasCheckConstraint("CK_Advertisements_Dates", "\"EndDate\" IS NULL OR \"StartDate\" IS NULL OR \"EndDate\" >= \"StartDate\""));
        UserConfiguration.ConfigureEntity(builder);
        builder.Property(x => x.TitleAr).HasMaxLength(300).IsRequired();
        builder.Property(x => x.TitleEn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ImageUrl).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.MobileImageUrl).HasMaxLength(2048);
        builder.Property(x => x.LinkUrl).HasMaxLength(2048);
        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder });
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        UserConfiguration.ConfigureEntity(builder);
        builder.Property(x => x.Action).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Metadata).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
