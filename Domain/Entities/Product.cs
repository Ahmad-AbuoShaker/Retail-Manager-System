using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; } 
        public string Description { get; set; } 
        public int QuantityInStock { get; set; }
        public decimal RetailPrice { get; set; }
        public bool IsTaxable { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }

        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }
}
