using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ProductName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.RetailPrice).IsRequired();
        builder.Property(p => p.IsTaxable).IsRequired().HasDefaultValue(true);

        builder.Property(p => p.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(p => p.LastModified).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(p => p.QuantityInStock).IsRequired().HasDefaultValue(0);

        builder.ToTable(t => t.HasCheckConstraint("CK_Products_QuantityInStock_NonNegative", "[QuantityInStock] >= 0"));
    }
}


