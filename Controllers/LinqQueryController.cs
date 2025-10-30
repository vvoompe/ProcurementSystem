using ProcurementSystem.Data;
using ProcurementSystem.Models;
using ProcurementSystem.ViewModels;
using System.Linq;
using System;
using System.Web.Mvc;
using System.Data.Entity;
using System.Diagnostics;

namespace ProcurementSystem.Controllers
{
    public class LinqQueryController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: LinqQuery
        public ActionResult Index()
        {
            ViewBag.ExpensiveProducts = db.Products
                                          .Include(p => p.Category)
                                          .Where(p => p.Price > 50000)
                                          .ToList();

            ViewBag.Laptops = db.Products
                                .Include(p => p.Category)
                                .Where(p => p.Category.Name == "Ноутбуки")
                                .ToList();

            ViewBag.TotalOrdersSum = db.Orders.Sum(o => o.TotalAmount);

            ViewBag.ProductsPerCategory = db.Categories
                                            .Select(c => new CategoryProductCountViewModel
                                            {
                                                CategoryName = c.Name,
                                                ProductCount = c.Products.Count()
                                            })
                                            .ToList();

            ViewBag.SuppliersWithNoOffers = db.Suppliers
                                              .Where(s => !s.Offers.Any())
                                              .ToList();

            ViewBag.CreateMessage = DemoCreate();
            ViewBag.UpdateMessage = DemoUpdate();
            ViewBag.DeleteMessage = DemoDelete();

            return View();
        }

        private string DemoCreate()
        {
            try
            {
                var monitorCategory = db.Categories.FirstOrDefault(c => c.Name == "Монітори");

                var existingProduct = db.Products.FirstOrDefault(p => p.Name == "Демо Товар (для ЛР)");

                if (existingProduct == null && monitorCategory != null)
                {
                    var newProduct = new Product
                    {
                        Name = "Демо Товар (для ЛР)",
                        Description = "Цей товар створено автоматично для демонстрації LINQ Add",
                        Price = 100,
                        Stock = 10,
                        CategoryId = monitorCategory.Id
                    };
                    db.Products.Add(newProduct);
                    db.SaveChanges();

                    return "Успіх (Додавання): Створено 'Демо Товар (для ЛR)'.";
                }
                return "Інфо (Додавання): 'Демо Товар (для ЛР)' вже існує.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return "Помилка (Додавання): Не вдалося створити товар.";
            }
        }
        private string DemoUpdate()
        {
            try
            {
                var productToUpdate = db.Products.FirstOrDefault(p => p.Name == "Демо Товар (для ЛР)");

                if (productToUpdate != null)
                {
                    productToUpdate.Price = 150;
                    productToUpdate.Description = "Цей товар ОНОВЛЕНО для демонстрації LINQ Update";

                    db.Entry(productToUpdate).State = EntityState.Modified;

                    db.SaveChanges();

                    return "Успіх (Оновлення): Оновлено ціну 'Демо Товар (для ЛР)' до 150 грн.";
                }

                return "Інфо (Оновлення): Товар для оновлення не знайдено (можливо, він ще не створений).";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return "Помилка (Оновлення): Не вдалося оновити товар.";
            }
        }
        private string DemoDelete()
        {
            try
            {
                var productToDelete = db.Products.FirstOrDefault(p =>
                    p.Name == "Демо Товар (для ЛР)" && p.Price == 150);

                if (productToDelete != null)
                {
                    db.Products.Remove(productToDelete);

                    db.SaveChanges();

                    return "Успіх (Видалення): 'Демо Товар (для ЛР)' видалено.";
                }

                return "Інфо (Видалення): Товар для видалення не знайдено.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return "Помилка (Видалення): Не вдалося видалити товар.";
            }
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