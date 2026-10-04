using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal RetailPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }

        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }
}
