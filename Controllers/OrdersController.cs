using ProcurementSystem.Data; 
using ProcurementSystem.Models; 
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using ProcurementSystem.Models.Enums;   
using System.Net; // Добавлено для HttpStatusCode

namespace ProcurementSystem.Controllers
{
    public class OrdersController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: Orders
        public ActionResult Index()
        {
            // Переконайтесь, що ви завантажуєте User, щоб уникнути помилки в View
            var orders = db.Orders.Include(o => o.User);
            return View(orders.ToList());
        }

        // GET: Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Orders/Create
        public ActionResult Create()
        {
            // Передаємо список користувачів для випадаючого списку у View, якщо він потрібен
            ViewBag.UserId = new SelectList(db.Users, "Id", "Login"); // Используем "Login"
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Order order)
        {
            // Встановлюємо значення за замовчуванням
            order.UserId = 1; // Замініть на логіку отримання ID поточного користувача
            order.TotalAmount = 0;

            // Видаляємо ці поля з перевірки ModelState
            ModelState.Remove("UserId");
            ModelState.Remove("TotalAmount");

            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Якщо ModelState не валідний, повертаємо форму з помилками
            ViewBag.UserId = new SelectList(db.Users, "Id", "Login", order.UserId); // Используем "Login"
            return View(order);
        }

        // GET: Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "Login", order.UserId); // Используем "Login"
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,OrderDate,Status,TotalAmount,UserId")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "Login", order.UserId); // Используем "Login"
            return View(order);
        }

        // GET: Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
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