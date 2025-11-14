using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcurementSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProcurementSystem.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public virtual Order Order { get; set; }
    }
}