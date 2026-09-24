using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
      

        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            throw new NotImplementedException();
        }
    }

}


