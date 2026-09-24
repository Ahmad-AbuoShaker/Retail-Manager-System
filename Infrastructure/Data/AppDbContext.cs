using Application.Common.Interface;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interface;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext, IAppDbContex
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventorys { get; set; }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IAssemblyMaker).Assembly);
        }

        public async Task<int> SaveChangeAsync(CancellationToken cancellationToken)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
