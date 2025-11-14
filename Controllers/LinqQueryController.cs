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
using ProcurementSystem.Models.Enums;

namespace ProcurementSystem.Controllers
{
    public class LinqQueryController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: LinqQuery
        public ActionResult Index()
        {
            // Query 1: >50000
            var expensiveProducts = db.Products
                .Include(p => p.Category)
                .Where(p => p.Price > 50000m)
                .ToList();
            ViewBag.ExpensiveProducts = expensiveProducts;

            // Query 2: "Ноутбуки"
            var laptops = db.Products
                .Include(p => p.Category)
                .Where(p => p.Category.Name == "Ноутбуки")
                .ToList();
            ViewBag.Laptops = laptops;

            // Query 3: Sum
            var totalSum = db.Orders.Select(o => (decimal?)o.TotalAmount).Sum() ?? 0m;
            ViewBag.TotalOrdersSum = totalSum;

            // Query 4: items per category
            var productsPerCategory = db.Categories
                .Select(c => new CategoryProductCountViewModel
                {
                    CategoryName = c.Name,
                    ProductCount = c.Products.Count()
                })
                .ToList();
            ViewBag.ProductsPerCategory = productsPerCategory;

            // Query 5: Постачальники без пропозицій
            var suppliersWithNoOffers = db.Suppliers
                .Where(s => !s.Offers.Any())
                .ToList();
            ViewBag.SuppliersWithNoOffers = suppliersWithNoOffers;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "АДМІНІСТРАТОР")] 
        public ActionResult AddUser(string login, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    TempData["UserError"] = "Логін та пароль не можуть бути порожніми.";
                    return RedirectToAction("Index");
                }

                if (db.Users.Any(u => u.Login == login))
                {
                    TempData["UserError"] = $"Користувач з логіном '{login}' вже існує.";
                    return RedirectToAction("Index");
                }

                var newUser = new User
                {
                    Login = login,
                    Password = password,
                    Role = UserRole.МЕНЕДЖЕР
                };

                db.Users.Add(newUser);

                db.SaveChanges();

                TempData["UserSuccess"] = $"Користувача '{login}' успішно додано!";
            }
            catch (Exception ex)
            {
                TempData["UserError"] = "Помилка при додаванні користувача: " + ex.Message;
            }


            return RedirectToAction("Index");
        }
    }
}