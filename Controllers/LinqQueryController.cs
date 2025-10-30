using ProcurementSystem.Data;
using ProcurementSystem.Models; // Потрібно для Supplier
using ProcurementSystem.ViewModels;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity; // <- Дуже важливо для .Include()

namespace ProcurementSystem.Controllers
{
    public class LinqQueryController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: LinqQuery
        public ActionResult Index()
        {
            // 1. (Фільтрація) Товари, дорожчі за 50 000 грн
            // Додаємо .Include(p => p.Category), щоб у View спрацювало @item.Category.Name
            ViewBag.ExpensiveProducts = db.Products
                                          .Include(p => p.Category)
                                          .Where(p => p.Price > 50000)
                                          .ToList();

            // 2. (Фільтрація) Всі товари з категорії "Ноутбуки"
            // Також додаємо .Include(p => p.Category)
            ViewBag.Laptops = db.Products
                                .Include(p => p.Category)
                                .Where(p => p.Category.Name == "Ноутбуки")
                                .ToList();

            // 3. (Узагальнення - Sum) Загальна сума всіх замовлень
            // .Sum() для non-nullable decimal поверне 0.0m, якщо замовлень немає,
            // а не null. Тож (decimal) у View буде безпечним.
            ViewBag.TotalOrdersSum = db.Orders.Sum(o => o.TotalAmount);

            // 4. (Узагальнення - GroupBy/Count) Кількість товарів по категоріях
            // Використовуємо навігаційну властивість - це надійно.
            ViewBag.ProductsPerCategory = db.Categories
                                            .Select(c => new CategoryProductCountViewModel
                                            {
                                                CategoryName = c.Name,
                                                ProductCount = c.Products.Count()
                                            })
                                            .ToList();

            // 5. (Фільтрація - Any) Постачальники без пропозицій
            // Використовуємо навігаційну властивість SupplierOffers
            ViewBag.SuppliersWithNoOffers = db.Suppliers
                                              .Where(s => !s.Offers.Any())
                                              .ToList();

            // Повертаємо View. Всі дані передаються через ViewBag.
            return View();
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