using System;
using System.Collections.Generic;

namespace ProcurementSystem.ViewModels
{
    public class QuantityReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ProductQuantityReportItem> Items { get; set; }
    }

    public class ProductQuantityReportItem
    {
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}