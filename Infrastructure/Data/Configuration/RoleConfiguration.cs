using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);

        builder.HasIndex(r => r.Name).IsUnique();

        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Role { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Cashier" },
            new Role { Id = new Guid("22222222-2222-2222-2222-222222222222"), Name = "Admin" },
            new Role { Id = new Guid("33333333-3333-3333-3333-333333333333"), Name = "WarehouseWorker" },
            new Role { Id = new Guid("44444444-4444-4444-4444-444444444444"), Name = "Manager" }
        );
    }
}