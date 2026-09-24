using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Sale
    {
        public int SaleId {  get; set; }
        public string UserID {  get; set; }
        public DateTime SaleDate {  get; set; }
        public decimal SubTotal {  get; set; }
        public decimal Tax {  get; set; }
        public decimal Total {  get; set; }
    }
}
