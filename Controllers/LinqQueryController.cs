using ProcurementSystem.Data; // Добавлено: содержит ProcurementContext
using ProcurementSystem.Models;
using ProcurementSystem.ViewModels;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace ProcurementSystem.Controllers
{
    public class LinqQueryController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        public ActionResult Index()
        {
            var expensiveProducts = db.Products
                .Where(p => p.Price > 50000)
                .Include(p => p.Category)
                .ToList();

            var laptops = db.Products
                .Where(p => p.Category.Name == "Ноутбуки")
                .Include(p => p.Category)
                .ToList();

            decimal totalOrdersSum = db.Orders.Sum(o => o.TotalAmount);

            var productsPerCategory = db.Categories
                .Select(c => new CategoryProductCountViewModel
                {
                    CategoryName = c.Name,
                    ProductCount = c.Products.Count()
                })
                .ToList();

            var suppliersWithNoOffers = db.Suppliers
                .Where(s => !s.Offers.Any())
                .ToList();

            ViewBag.ExpensiveProducts = expensiveProducts;
            ViewBag.Laptops = laptops;
            ViewBag.TotalOrdersSum = totalOrdersSum;
            ViewBag.ProductsPerCategory = productsPerCategory;
            ViewBag.SuppliersWithNoOffers = suppliersWithNoOffers;

            return View();
        }
    }
}