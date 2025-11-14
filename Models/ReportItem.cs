namespace ProcurementSystem.Models
{
    public class ReportItem
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public virtual Report Report { get; set; }

        public string CategoryName { get; set; }
        public string ProductName { get; set; }
        public string SupplierName { get; set; }

        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? AveragePrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}