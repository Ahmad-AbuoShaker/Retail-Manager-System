using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductName {  get; set; }
        public string Description {  get; set; }
        public decimal RetailPrice {  get; set; }
        public DateTime CreatDate {  get; set; }
        public DateTime LastModified {  get; set; }


    }
}
