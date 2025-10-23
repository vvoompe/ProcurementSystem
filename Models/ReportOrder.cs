using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProcurementSystem.Models
{
    public class ReportOrder
    {
        public int Id { get; set; }
        public DateTime GenerationDate { get; set; }

        public int ReportId { get; set; }
        public virtual Report Report { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
    }
}