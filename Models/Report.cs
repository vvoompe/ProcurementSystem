using ProcurementSystem.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProcurementSystem.Models
{
    public class Report
    {
        public int Id { get; set; }

        [Display(Name = "Тип Звіту")]
        public ReportType ReportType { get; set; }

        [Display(Name = "Створено")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Створив")]
        public string CreatedByLogin { get; set; }

        [Display(Name = "Початок Періоду")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Кінець Періоду")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Загальна Сума")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal GrandTotal { get; set; }

        public virtual ICollection<ReportItem> ReportItems { get; set; }
    }
}