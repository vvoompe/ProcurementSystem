using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProcurementSystem.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Contacts { get; set; }

        public virtual ICollection<SupplierOffer> Offers { get; set; }
    }
}