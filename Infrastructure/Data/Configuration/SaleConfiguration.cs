using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SaleDate).IsRequired();
        builder.Property(s => s.SubTotal).IsRequired();
        builder.Property(s => s.Tax).IsRequired();
        builder.Property(s => s.Total).IsRequired();

        builder.HasOne(s => s.User)
            .WithMany(u => u.Sales)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Sales_SubTotal_NonNegative", "[SubTotal] >= 0");
            t.HasCheckConstraint("CK_Sales_Tax_NonNegative", "[Tax] >= 0");
            t.HasCheckConstraint("CK_Sales_Total_NonNegative", "[Total] >= 0");
        });
    }
}


