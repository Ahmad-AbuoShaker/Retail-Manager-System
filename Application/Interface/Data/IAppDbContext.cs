using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interface.Data;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleDetail> SaleDetails { get; }
    DbSet<Product> Products { get; }
    DbSet<Inventory> Inventories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
