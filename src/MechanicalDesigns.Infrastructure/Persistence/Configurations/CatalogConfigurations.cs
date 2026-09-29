using MechanicalDesigns.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicalDesigns.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", table => table.HasCheckConstraint("CK_Categories_DeletedAt", "\"IsDeleted\" = FALSE OR \"DeletedAt\" IS NOT NULL"));
        UserConfiguration.ConfigureEntity(builder);
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(220).IsRequired();
        builder.Property(x => x.ImageUrl).HasMaxLength(2048);
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.IsActive);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", table =>
        {
            table.HasCheckConstraint("CK_Products_Price", "\"Price\" >= 0");
            table.HasCheckConstraint("CK_Products_StockQuantity", "\"StockQuantity\" >= 0");
        });
        UserConfiguration.ConfigureEntity(builder);
        builder.Property(x => x.TitleAr).HasMaxLength(300).IsRequired();
        builder.Property(x => x.TitleEn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.SubtitleAr).HasMaxLength(500);
        builder.Property(x => x.SubtitleEn).HasMaxLength(500);
        builder.Property(x => x.DescriptionAr).HasMaxLength(10000);
        builder.Property(x => x.DescriptionEn).HasMaxLength(10000);
        builder.Property(x => x.Price).HasPrecision(12, 2);
        builder.Property(x => x.Slug).HasMaxLength(320).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.CategoryId, x.IsActive });
        builder.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        UserConfiguration.ConfigureEntity(builder);
        builder.Property(x => x.ImageUrl).HasMaxLength(2048).IsRequired();
        builder.HasIndex(x => new { x.ProductId, x.DisplayOrder });
        builder.HasOne(x => x.Product).WithMany(x => x.Images).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(x => !x.Product.IsDeleted);
    }
}

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews", table => table.HasCheckConstraint("CK_Reviews_Rating", "\"Rating\" BETWEEN 1 AND 5"));
        UserConfiguration.ConfigureEntity(builder);
        builder.Property(x => x.Comment).HasMaxLength(3000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
        builder.HasIndex(x => new { x.ProductId, x.Status });
        builder.HasOne(x => x.Product).WithMany(x => x.Reviews).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.User).WithMany(x => x.Reviews).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.Product.IsDeleted);
    }
}
