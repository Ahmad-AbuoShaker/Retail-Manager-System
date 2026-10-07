using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.HasKey(sd => sd.Id);
        builder.Property(sd => sd.Quantity).IsRequired().HasDefaultValue(1);
        builder.Property(sd => sd.UnitPrice).IsRequired();
        builder.Property(sd => sd.Tax).IsRequired().HasDefaultValue(0m);

        builder.HasOne(sd => sd.Sale)
            .WithMany(s => s.SaleDetails)
            .HasForeignKey(sd => sd.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sd => sd.Product)
            .WithMany(p => p.SaleDetails)
            .HasForeignKey(sd => sd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_SaleDetails_Quantity_NonNegative", "[Quantity] >= 0");
            t.HasCheckConstraint("CK_SaleDetails_UnitPrice_NonNegative", "[UnitPrice] >= 0");
            t.HasCheckConstraint("CK_SaleDetails_Tax_NonNegative", "[Tax] >= 0");
        });
    }
}


