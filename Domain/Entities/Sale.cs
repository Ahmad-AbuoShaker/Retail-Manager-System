using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Sale
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }

        public User User { get; set; } = null!;
        public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }
}
