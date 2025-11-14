using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ProcurementSystem.ViewModels;
using ProcurementSystem.Models;

namespace ProcurementSystem.Controllers
{
    [Authorize(Roles = "МЕНЕДЖЕР, АДМІНІСТРАТОР")]
    public class ReportsController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: Reports
        // Це буде наша головна сторінка для вибору звіту
        public ActionResult Index()
        {
            var viewModel = new ReportDateRangeViewModel();
            return View(viewModel);
        }

        // POST: Reports/FinancialReport
        // Генерує фінансовий звіт
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinancialReport(ReportDateRangeViewModel range)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", range);
            }

            // Встановлюємо кінець дня для EndDate, щоб включити всі замовлення за цей день
            var endDate = range.EndDate.Date.AddDays(1).AddTicks(-1);
            var startDate = range.StartDate.Date;

            var orderItems = db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Offer.Product.Category)
                .Include(oi => oi.Offer.Supplier)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate)
                .OrderBy(oi => oi.Offer.Product.Category.Name)
                .ThenBy(oi => oi.Offer.Product.Name)
                .ToList();

            var report = new FinancialReportViewModel
            {
                StartDate = startDate,
                EndDate = range.EndDate.Date,
                TotalGrandAmount = orderItems.Sum(oi => oi.Amount)
            };

            var groupedByCateogry = orderItems.GroupBy(oi => oi.Offer.Product.Category);

            foreach (var categoryGroup in groupedByCateogry)
            {
                var categoryReport = new CategoryFinancialReport
                {
                    CategoryName = categoryGroup.Key?.Name ?? "Без категорії",
                    TotalCategoryAmount = categoryGroup.Sum(oi => oi.Amount)
                };

                foreach (var item in categoryGroup)
                {
                    categoryReport.Items.Add(new ReportOrderItemDetail
                    {
                        ProductName = item.Offer.Product.Name,
                        SupplierName = item.Offer.Supplier.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalAmount = item.Amount
                    });
                }
                report.Categories.Add(categoryReport);
            }

            return View("FinancialReport", report);
        }

        // POST: Reports/QuantityReport
        // Генерує звіт по кількості
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuantityReport(ReportDateRangeViewModel range)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", range);
            }

            var endDate = range.EndDate.Date.AddDays(1).AddTicks(-1);
            var startDate = range.StartDate.Date;

            var orderItems = db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Offer.Product.Category)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate)
                .ToList();

            var groupedByProduct = orderItems
                .GroupBy(oi => oi.Offer.Product)
                .Select(g => new ProductQuantityReportItem
                {
                    ProductName = g.Key.Name,
                    CategoryName = g.Key.Category?.Name ?? "Без категорії",
                    TotalQuantitySold = g.Sum(oi => oi.Quantity),
                    TotalAmount = g.Sum(oi => oi.Amount),
                    AveragePrice = g.Sum(oi => oi.Amount) / g.Sum(oi => oi.Quantity)
                })
                .OrderBy(r => r.CategoryName)
                .ThenBy(r => r.ProductName)
                .ToList();

            var report = new QuantityReportViewModel
            {
                StartDate = startDate,
                EndDate = range.EndDate.Date,
                Items = groupedByProduct
            };

            return View("QuantityReport", report);
        }


        // Позбуваємося старих методів, оскільки моделі Report і ReportOrder більше не потрібні
        // Details, Create, Edit, Delete ... видалено ...


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}