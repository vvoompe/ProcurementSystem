using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProcurementSystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва товару є обов'язковою.")]
        public string Name { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "Ціна є обов'язковою.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ціна має бути додатним числом (більше 0.01).")]
        [DisplayFormat(DataFormatString = "{0:0.00}")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Кількість на складі є обов'язковою.")]
        [Range(0, int.MaxValue, ErrorMessage = "Кількість на складі не може бути від'ємною.")]
        public int Stock { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public virtual ICollection<SupplierOffer> SupplierOffers { get; set; }
    }
}