using MechanicalDesigns.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicalDesigns.Infrastructure.Persistence.Configurations;

public sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.FullName).HasMaxLength(150).IsRequired();
        builder.Property(m => m.Email).HasMaxLength(254).IsRequired();
        builder.Property(m => m.PhoneNumber).HasMaxLength(11).IsRequired();
        builder.Property(m => m.Subject).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Message).HasMaxLength(5000).IsRequired();
        builder.HasIndex(m => new { m.CreatedAt, m.Id });
    }
}
