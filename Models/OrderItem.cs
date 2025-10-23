using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProcurementSystem.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }  
        public decimal Amount { get; set; } 

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int SupplierOfferId { get; set; }
        public virtual SupplierOffer Offer { get; set; }
    }
}