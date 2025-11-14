using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcurementSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProcurementSystem.Models
{
    public class Order
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Опис")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}