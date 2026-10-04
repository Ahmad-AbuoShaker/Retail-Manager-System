using System;

namespace Domain.Entities
{
    public class SaleDetail
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal Tax { get; set; }

        public Sale Sale { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
