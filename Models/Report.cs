using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProcurementSystem.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Period { get; set; }
        public string Type { get; set; }

        public virtual ICollection<ReportOrder> ReportOrders { get; set; }
    }
}