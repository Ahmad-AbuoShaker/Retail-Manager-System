using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Interfaces.Data;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleDetail> SaleDetails { get; }
    DbSet<Product> Products { get; }
    DbSet<Inventory> Inventories { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
