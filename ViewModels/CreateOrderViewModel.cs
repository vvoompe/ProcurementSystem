using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ProcurementSystem.ViewModels
{
    public class CreateOrderViewModel
    {
        [Required(ErrorMessage = "Будь ласка, введіть опис заявки.")]
        [Display(Name = "Опис Заявки")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Будь ласка, оберіть товар.")]
        [Display(Name = "Оберіть Товар (Пропозицію)")]
        public int SupplierOfferId { get; set; }

        [Required(ErrorMessage = "Будь ласка, вкажіть кількість.")]
        [Range(1, int.MaxValue, ErrorMessage = "Кількість має бути принаймні 1.")]
        [Display(Name = "Кількість")]
        public int Quantity { get; set; }

        public SelectList OfferList { get; set; }
    }
}