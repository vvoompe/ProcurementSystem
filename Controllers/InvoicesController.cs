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
using ProcurementSystem.Models.Enums; // Додано для доступу до PaymentStatus

namespace ProcurementSystem.Controllers
{
    // 1. Додано: Захист всього контролера
    [Authorize(Roles = "БУХГАЛТЕР, МЕНЕДЖЕР, АДМІНІСТРАТОР")]
    public class InvoicesController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: Invoices
        public ActionResult Index()
        {
            // Включаємо пов'язані дані 'Order' для відображення в таблиці
            var invoices = db.Invoices.Include(i => i.Order);
            return View(invoices.ToList());
        }

        // GET: Invoices/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Включаємо дані про Замовлення та Користувача
            Invoice invoice = db.Invoices
                                .Include(i => i.Order.User)
                                .FirstOrDefault(i => i.Id == id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // GET: Invoices/Create
        public ActionResult Create()
        {
            // 2. Покращено: Робимо список Замовлень (Orders) більш інформативним
            var ordersList = db.Orders
                .Include(o => o.User)
                .Where(o => o.Status != OrderStatus.СКАСОВАНО) // Не можна створювати рахунки для скасованих заявок
                .AsEnumerable() // Переходимо до обробки в пам'яті
                .Select(o => new {
                    Id = o.Id,
                    Name = $"Заявка №{o.Id} (Співробітник: {o.User?.Login ?? "N/A"}, Опис: {o.Description})"
                }).ToList();

            ViewBag.OrderId = new SelectList(ordersList, "Id", "Name");

            // 3. Додано: Передаємо список статусів
            ViewBag.Status = new SelectList(
                Enum.GetValues(typeof(PaymentStatus))
                    .Cast<PaymentStatus>()
                    .Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id",
                "Name",
                (int)PaymentStatus.ОЧІКУЄТЬСЯ); // За замовчуванням "Очікується"

            return View();
        }

        // POST: Invoices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Amount,InvoiceDate,Status,OrderId")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                db.Invoices.Add(invoice);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // 4. Покращено: Повторно заповнюємо список Замовлень у разі помилки
            var ordersList = db.Orders
                .Include(o => o.User)
                .Where(o => o.Status != OrderStatus.СКАСОВАНО)
                .AsEnumerable()
                .Select(o => new {
                    Id = o.Id,
                    Name = $"Заявка №{o.Id} (Співробітник: {o.User?.Login ?? "N/A"}, Опис: {o.Description})"
                }).ToList();

            ViewBag.OrderId = new SelectList(ordersList, "Id", "Name", invoice.OrderId);

            // 5. Додано: Повторно передаємо список статусів
            ViewBag.Status = new SelectList(
                Enum.GetValues(typeof(PaymentStatus))
                    .Cast<PaymentStatus>()
                    .Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id",
                "Name",
                (int)invoice.Status);

            return View(invoice);
        }

        // GET: Invoices/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }

            // 6. Покращено: Заповнюємо список Замовлень
            var ordersList = db.Orders
                .Include(o => o.User)
                .AsEnumerable()
                .Select(o => new {
                    Id = o.Id,
                    Name = $"Заявка №{o.Id} (Співробітник: {o.User?.Login ?? "N/A"}, Опис: {o.Description})"
                }).ToList();

            ViewBag.OrderId = new SelectList(ordersList, "Id", "Name", invoice.OrderId);

            // 7. Додано: Передаємо список статусів
            ViewBag.Status = new SelectList(
                Enum.GetValues(typeof(PaymentStatus))
                    .Cast<PaymentStatus>()
                    .Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id",
                "Name",
                (int)invoice.Status);

            return View(invoice);
        }

        // POST: Invoices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Amount,InvoiceDate,Status,OrderId")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(invoice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // 8. Покращено: Повторно заповнюємо список Замовлень у разі помилки
            var ordersList = db.Orders
                .Include(o => o.User)
                .AsEnumerable()
                .Select(o => new {
                    Id = o.Id,
                    Name = $"Заявка №{o.Id} (Співробітник: {o.User?.Login ?? "N/A"}, Опис: {o.Description})"
                }).ToList();

            ViewBag.OrderId = new SelectList(ordersList, "Id", "Name", invoice.OrderId);

            // 9. Додано: Повторно передаємо список статусів
            ViewBag.Status = new SelectList(
                Enum.GetValues(typeof(PaymentStatus))
                    .Cast<PaymentStatus>()
                    .Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id",
                "Name",
                (int)invoice.Status);

            return View(invoice);
        }

        // GET: Invoices/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices
                                .Include(i => i.Order.User) // Включаємо дані
                                .FirstOrDefault(i => i.Id == id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }


            if (invoice.Status != PaymentStatus.ОЧІКУЄТЬСЯ)
            {
                ModelState.AddModelError("", $"Неможливо видалити рахунок зі статусом '{invoice.Status}'. Видаляти можна лише скасовані рахунки.");

                db.Entry(invoice).Reference(i => i.Order).Load();
                if (invoice.Order != null)
                    db.Entry(invoice.Order).Reference(o => o.User).Load();

                return View(invoice); 
            }

            db.Invoices.Remove(invoice);
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