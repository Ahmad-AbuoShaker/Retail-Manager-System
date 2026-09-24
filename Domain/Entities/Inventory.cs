using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Inventory
    {
        public int Id { get; set; }
        public int ProductId {  get; set; }
        public decimal PurchaesPrice {  get; set; }
        public DateTime PurchaesDate {  get; set; }
    }
}
