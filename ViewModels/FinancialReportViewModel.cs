using System;
using System.Collections.Generic;

namespace ProcurementSystem.ViewModels
{
    public class FinancialReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CategoryFinancialReport> Categories { get; set; }
        public decimal TotalGrandAmount { get; set; }

        public FinancialReportViewModel()
        {
            Categories = new List<CategoryFinancialReport>();
        }
    }

    public class CategoryFinancialReport
    {
        public string CategoryName { get; set; }
        public List<ReportOrderItemDetail> Items { get; set; }
        public decimal TotalCategoryAmount { get; set; }

        public CategoryFinancialReport()
        {
            Items = new List<ReportOrderItemDetail>();
        }
    }

    public class ReportOrderItemDetail
    {
        public string ProductName { get; set; }
        public string SupplierName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}