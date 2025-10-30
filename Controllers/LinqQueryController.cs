using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProcurementSystem;
using ProcurementSystem.Models;
using ProcurementSystem.ViewModels;

namespace ProcurementSystem.Controllers
{
    public class LinqQueryController : Controller
    {
        private ProcurementContext db = new ProcurementContext();
        // GET: LinqQuery
        public ActionResult Index()
        {
            // Query 1: Кількість продуктів у кожній категорії
            var categoriesWithProductCount = db.Categories
                .Select(c => new CategoryProductCountViewModel
                {
                    CategoryName = c.Name,
                    ProductCount = c.Products.Count()
                }).ToList();
            ViewBag.CategoriesWithProductCount = categoriesWithProductCount;

            // Query 2: Продукти з категоріями та постачальниками
            var productsWithCategoriesAndSuppliers = db.Products
                .Include(p => p.Category)
                .Include(p => p.SupplierOffers.Select(so => so.Supplier))
                .ToList();
            ViewBag.ProductsWithDetails = productsWithCategoriesAndSuppliers;


            // Query 3: Всі замовлення (Order)
            var allOrders = db.Orders.Include(o => o.User).ToList();
            ViewBag.AllOrders = allOrders;

            // Query 4: Всі позиції замовлень (OrderItem)
            var allOrderItems = db.OrderItems
                                .Include(oi => oi.Offer.Product)
                                .Include(oi => oi.Order)
                                .ToList();
            ViewBag.AllOrderItems = allOrderItems;


            return View();
        }
    }
}