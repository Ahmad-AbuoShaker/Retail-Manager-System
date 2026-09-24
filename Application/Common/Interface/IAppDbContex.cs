using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interface
{
    public interface IAppDbContex
    {
        public DbSet<User> Users { get; }
        public DbSet<Sale> Sales { get; }
        public DbSet<SaleDetail> SaleDetails { get;}
        public DbSet<Product> Products { get; }
        public DbSet<Inventory> Inventorys { get;  }

        Task<int> SaveChangeAsync(CancellationToken cancellationToken);
    }
}
