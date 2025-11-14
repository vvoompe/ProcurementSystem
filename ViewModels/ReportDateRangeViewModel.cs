using System;
using System.ComponentModel.DataAnnotations;

namespace ProcurementSystem.ViewModels
{
    public class ReportDateRangeViewModel
    {
        [Required(ErrorMessage = "Будь ласка, введіть початкову дату.")]
        [Display(Name = "Початкова Дата")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; } = DateTime.Now.Date.AddMonths(-1);

        [Required(ErrorMessage = "Будь ласка, введіть кінцеву дату.")]
        [Display(Name = "Кінцева Дата")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; } = DateTime.Now.Date.AddHours(23).AddMinutes(59);
    }
}