using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ProcurementSystem.ViewModels;
using ProcurementSystem.Models;
using ProcurementSystem.Enums;
using System.Collections.Generic;
using System.Net;

namespace ProcurementSystem.Controllers
{
    [Authorize(Roles = "МЕНЕДЖЕР, АДМІНІСТРАТОР")]
    public class ReportsController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: Reports
        // Тепер показує список збережених звітів
        public ActionResult Index()
        {
            var reports = db.Reports.OrderByDescending(r => r.CreatedDate).ToList();
            ViewBag.DateRangeViewModel = new ReportDateRangeViewModel();
            return View(reports);
        }

        // GET: Reports/Details/5
        // Показує збережений звіт
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Report report = db.Reports.Include(r => r.ReportItems).FirstOrDefault(r => r.Id == id);
            if (report == null)
            {
                return HttpNotFound();
            }

            // Збираємо ViewModel зі збережених даних
            if (report.ReportType == ReportType.Фінансовий)
            {
                var viewModel = new FinancialReportViewModel
                {
                    StartDate = report.StartDate,
                    EndDate = report.EndDate,
                    TotalGrandAmount = report.GrandTotal,
                    Categories = new List<CategoryFinancialReport>()
                };

                var groupedByCateogry = report.ReportItems.GroupBy(item => item.CategoryName);
                foreach (var categoryGroup in groupedByCateogry)
                {
                    var categoryReport = new CategoryFinancialReport
                    {
                        CategoryName = categoryGroup.Key,
                        TotalCategoryAmount = categoryGroup.Sum(item => item.TotalAmount),
                        Items = categoryGroup.Select(item => new ReportOrderItemDetail
                        {
                            ProductName = item.ProductName,
                            SupplierName = item.SupplierName,
                            Quantity = item.Quantity ?? 0,
                            UnitPrice = item.UnitPrice ?? 0,
                            TotalAmount = item.TotalAmount
                        }).ToList()
                    };
                    viewModel.Categories.Add(categoryReport);
                }
                return View("FinancialReport", viewModel);
            }

            if (report.ReportType == ReportType.Кількісний)
            {
                var viewModel = new QuantityReportViewModel
                {
                    StartDate = report.StartDate,
                    EndDate = report.EndDate,
                    Items = report.ReportItems.Select(item => new ProductQuantityReportItem
                    {
                        ProductName = item.ProductName,
                        CategoryName = item.CategoryName,
                        TotalQuantitySold = item.Quantity ?? 0,
                        AveragePrice = item.AveragePrice ?? 0,
                        TotalAmount = item.TotalAmount
                    }).ToList()
                };
                return View("QuantityReport", viewModel);
            }

            return HttpNotFound("Тип звіту невідомий.");
        }


        // POST: Reports/FinancialReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinancialReport(ReportDateRangeViewModel range)
        {
            if (!ModelState.IsValid)
            {
                var reports = db.Reports.OrderByDescending(r => r.CreatedDate).ToList();
                ViewBag.DateRangeViewModel = range;
                return View("Index", reports);
            }

            var startDate = range.StartDate;
            var endDate = range.EndDate;

            var orderItems = db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Offer.Product.Category)
                .Include(oi => oi.Offer.Supplier)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate)
                .ToList();

            var reportHeader = new Report
            {
                ReportType = ReportType.Фінансовий,
                CreatedDate = DateTime.Now,
                CreatedByLogin = User.Identity.Name,
                StartDate = startDate,
                EndDate = endDate,
                GrandTotal = orderItems.Sum(oi => oi.Amount),
                ReportItems = new List<ReportItem>()
            };

            var groupedByCateogry = orderItems.GroupBy(oi => oi.Offer.Product.Category);
            foreach (var categoryGroup in groupedByCateogry)
            {
                foreach (var item in categoryGroup)
                {
                    var reportItem = new ReportItem
                    {
                        CategoryName = categoryGroup.Key?.Name ?? "Без категорії",
                        ProductName = item.Offer.Product.Name,
                        SupplierName = item.Offer.Supplier.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalAmount = item.Amount
                    };
                    reportHeader.ReportItems.Add(reportItem);
                }
            }

            db.Reports.Add(reportHeader);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = reportHeader.Id });
        }

        // POST: Reports/QuantityReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuantityReport(ReportDateRangeViewModel range)
        {
            if (!ModelState.IsValid)
            {
                var reports = db.Reports.OrderByDescending(r => r.CreatedDate).ToList();
                ViewBag.DateRangeViewModel = range;
                return View("Index", reports);
            }

            var startDate = range.StartDate;
            var endDate = range.EndDate;

            var orderItems = db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Offer.Product.Category)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate)
                .ToList();

            var groupedByProduct = orderItems
                .GroupBy(oi => oi.Offer.Product)
                .Select(g => new
                {
                    Product = g.Key,
                    CategoryName = g.Key.Category?.Name ?? "Без категорії",
                    TotalQuantitySold = g.Sum(oi => oi.Quantity),
                    TotalAmount = g.Sum(oi => oi.Amount),
                    AveragePrice = g.Sum(oi => oi.Amount) / g.Sum(oi => oi.Quantity)
                })
                .ToList();

            var reportHeader = new Report
            {
                ReportType = ReportType.Кількісний,
                CreatedDate = DateTime.Now,
                CreatedByLogin = User.Identity.Name,
                StartDate = startDate,
                EndDate = endDate,
                GrandTotal = groupedByProduct.Sum(g => g.TotalAmount),
                ReportItems = new List<ReportItem>()
            };

            foreach (var item in groupedByProduct)
            {
                var reportItem = new ReportItem
                {
                    CategoryName = item.CategoryName,
                    ProductName = item.Product.Name,
                    Quantity = item.TotalQuantitySold,
                    AveragePrice = item.AveragePrice,
                    TotalAmount = item.TotalAmount
                };
                reportHeader.ReportItems.Add(reportItem);
            }

            db.Reports.Add(reportHeader);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = reportHeader.Id });
        }

        // GET: Reports/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Report report = db.Reports.Find(id);
            if (report == null)
            {
                return HttpNotFound();
            }
            return View(report);
        }

        // POST: Reports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Report report = db.Reports.Include(r => r.ReportItems).FirstOrDefault(r => r.Id == id);
            if (report == null)
            {
                return HttpNotFound();
            }

            db.ReportItems.RemoveRange(report.ReportItems);
            db.Reports.Remove(report);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


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