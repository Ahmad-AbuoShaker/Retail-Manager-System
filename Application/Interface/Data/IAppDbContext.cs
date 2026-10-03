using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interface.Data
{
    public interface IAppDbContext
    {
        public DbSet<User> Users { get; }
        public DbSet<Sale> Sales { get; }
        public DbSet<SaleDetail> SaleDetails { get;}
        public DbSet<Product> Products { get; }
        public DbSet<Inventory> Inventorys { get;  }

        Task<int> SaveChangeAsync(CancellationToken cancellationToken);
    }
}
