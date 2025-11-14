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
using ProcurementSystem.Models.Enums;

namespace ProcurementSystem.Controllers
{
    [Authorize(Roles = "МЕНЕДЖЕР, АДМІНІСТРАТОР")]
    public class OrderItemsController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: OrderItems
        public ActionResult Index()
        {
            var orderItems = db.OrderItems.Include(o => o.Offer.Product).Include(o => o.Order.User);
            return View(orderItems.ToList());
        }

        // GET: OrderItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderItem orderItem = db.OrderItems
                                    .Include(o => o.Offer.Product)
                                    .Include(o => o.Offer.Supplier)
                                    .Include(o => o.Order.User)
                                    .FirstOrDefault(o => o.Id == id);
            if (orderItem == null)
            {
                return HttpNotFound();
            }
            return View(orderItem);
        }

        // --- УПРАВЛІННЯ ПОЗИЦІЯМИ ---
        // Створення (Create) та Редагування (Edit) позицій напряму - ДУЖЕ НЕБЕЗПЕЧНО.
        // Вони не оновлюють TotalAmount в Order і не керують Product.Stock.
        // Ми залишаємо ці методи, але вони несуть ризик для цілісності даних.
        // В ідеальному світі ці методи мали б бути видалені,
        // а управління позиціями - відбуватися ТІЛЬКИ через OrdersController.

        // GET: OrderItems/Create
        public ActionResult Create()
        {
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "Description");
            ViewBag.SupplierOfferId = new SelectList(db.SupplierOffers, "Id", "Id");
            return View();
        }

        // POST: OrderItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Quantity,UnitPrice,Amount,OrderId,SupplierOfferId")] OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                db.OrderItems.Add(orderItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OrderId = new SelectList(db.Orders, "Id", "Description", orderItem.OrderId);
            ViewBag.SupplierOfferId = new SelectList(db.SupplierOffers, "Id", "Id", orderItem.SupplierOfferId);
            return View(orderItem);
        }

        // GET: OrderItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderItem orderItem = db.OrderItems.Find(id);
            if (orderItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "Description", orderItem.OrderId);
            ViewBag.SupplierOfferId = new SelectList(db.SupplierOffers, "Id", "Id", orderItem.SupplierOfferId);
            return View(orderItem);
        }

        // POST: OrderItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Quantity,UnitPrice,Amount,OrderId,SupplierOfferId")] OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(orderItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "Description", orderItem.OrderId);
            ViewBag.SupplierOfferId = new SelectList(db.SupplierOffers, "Id", "Id", orderItem.SupplierOfferId);
            return View(orderItem);
        }

        // GET: OrderItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderItem orderItem = db.OrderItems
                                    .Include(o => o.Order)
                                    .Include(o => o.Offer.Product)
                                    .FirstOrDefault(o => o.Id == id);
            if (orderItem == null)
            {
                return HttpNotFound();
            }
            return View(orderItem);
        }

        // POST: OrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OrderItem orderItem = db.OrderItems
                                    .Include(o => o.Order)
                                    .Include(o => o.Offer.Product)
                                    .FirstOrDefault(o => o.Id == id);

            if (orderItem == null)
            {
                return HttpNotFound();
            }

            if (orderItem.Order.Status == OrderStatus.ВІДПРАВЛЕНО || orderItem.Order.Status == OrderStatus.ДОСТАВЛЕНО)
            {
                ModelState.AddModelError("", "Неможливо видалити позицію з активного (Відправлено) або завершеного (Доставлено) замовлення.");
                return View(orderItem);
            }

            db.OrderItems.Remove(orderItem);
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