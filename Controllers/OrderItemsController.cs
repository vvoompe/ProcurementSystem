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

        // GET: OrderItems/Create
        public ActionResult Create()
        {
            // Для зручності виводимо ID замовлення та опис (або ім'я користувача)
            ViewBag.OrderId = new SelectList(db.Orders.Include(o => o.User), "Id", "Description");

            // Формуємо гарний список товарів: "Назва товару (Постачальник) - Ціна"
            var offers = db.SupplierOffers
                           .Include(so => so.Product)
                           .Include(so => so.Supplier)
                           .ToList()
                           .Select(s => new
                           {
                               Id = s.Id,
                               Text = $"{s.Product.Name} ({s.Supplier.Name}) - {s.Price:C}"
                           });

            ViewBag.SupplierOfferId = new SelectList(offers, "Id", "Text");
            return View();
        }

        // POST: OrderItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Прибираємо UnitPrice та Amount з Bind, бо ми їх розраховуємо самі
        public ActionResult Create([Bind(Include = "Id,Quantity,OrderId,SupplierOfferId")] OrderItem orderItem)
        {
            // 1. Завантажуємо пов'язані сутності
            var order = db.Orders.Include(o => o.Invoices).FirstOrDefault(o => o.Id == orderItem.OrderId);
            var offer = db.SupplierOffers.Include(so => so.Product).FirstOrDefault(so => so.Id == orderItem.SupplierOfferId);

            if (order == null) ModelState.AddModelError("OrderId", "Замовлення не знайдено.");
            if (offer == null) ModelState.AddModelError("SupplierOfferId", "Товар не знайдено.");

            if (ModelState.IsValid)
            {
                // 2. Перевірка статусу замовлення
                if (order.Status == OrderStatus.ВІДПРАВЛЕНО || order.Invoices.Any(i => i.PaymentStatus == PaymentStatus.ОПЛАЧЕНО))
                {
                    ModelState.AddModelError("", "Неможливо додати позицію до замовлення, яке вже відправлено або має оплачені рахунки.");
                }
                // 3. Перевірка наявності на складі
                else if (offer.Product.Stock < orderItem.Quantity)
                {
                    ModelState.AddModelError("Quantity", $"Недостатньо товару на складі. Доступно: {offer.Product.Stock}");
                }
                else
                {
                    // 4. Логіка створення та оновлення

                    // Встановлюємо ціну та суму
                    orderItem.UnitPrice = offer.Price;
                    orderItem.Amount = orderItem.Quantity * offer.Price;

                    // Списуємо зі складу
                    var product = offer.Product;
                    product.Stock -= orderItem.Quantity;
                    db.Entry(product).State = EntityState.Modified;

                    // Оновлюємо загальну суму замовлення
                    order.TotalAmount += orderItem.Amount;
                    db.Entry(order).State = EntityState.Modified;

                    // Зберігаємо нову позицію
                    db.OrderItems.Add(orderItem);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            // Якщо сталася помилка, відновлюємо списки
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "Description", orderItem.OrderId);

            var offersList = db.SupplierOffers
                           .Include(so => so.Product)
                           .Include(so => so.Supplier)
                           .ToList()
                           .Select(s => new
                           {
                               Id = s.Id,
                               Text = $"{s.Product.Name} ({s.Supplier.Name}) - {s.Price:C}"
                           });
            ViewBag.SupplierOfferId = new SelectList(offersList, "Id", "Text", orderItem.SupplierOfferId);

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

            var offers = db.SupplierOffers
               .Include(so => so.Product)
               .Include(so => so.Supplier)
               .ToList()
               .Select(s => new
               {
                   Id = s.Id,
                   Text = $"{s.Product.Name} ({s.Supplier.Name}) - {s.Price:C}"
               });
            ViewBag.SupplierOfferId = new SelectList(offers, "Id", "Text", orderItem.SupplierOfferId);

            return View(orderItem);
        }

        // POST: OrderItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Quantity,UnitPrice,Amount,OrderId,SupplierOfferId")] OrderItem orderItem)
        {
            // 1. Отримуємо старі дані з БД (без відстеження)
            var originalItem = db.OrderItems
                                 .Include(oi => oi.Order.Invoices)
                                 .Include(oi => oi.Offer.Product)
                                 .AsNoTracking()
                                 .FirstOrDefault(oi => oi.Id == orderItem.Id);

            if (originalItem == null)
            {
                return HttpNotFound();
            }

            // 2. Перевірка: чи можна редагувати замовлення
            if (originalItem.Order.Status == OrderStatus.ВІДПРАВЛЕНО || originalItem.Order.Invoices.Any(i => i.PaymentStatus == PaymentStatus.ОПЛАЧЕНО))
            {
                ModelState.AddModelError("", "Неможливо редагувати позиції відправленого або оплаченого замовлення.");

                // Відновлення View для показу помилки
                ViewBag.OrderId = new SelectList(db.Orders, "Id", "Description", orderItem.OrderId);
                var offers = db.SupplierOffers.Include(so => so.Product).Include(so => so.Supplier).ToList().Select(s => new { Id = s.Id, Text = $"{s.Product.Name} ({s.Supplier.Name}) - {s.Price:C}" });
                ViewBag.SupplierOfferId = new SelectList(offers, "Id", "Text", orderItem.SupplierOfferId);
                return View(orderItem);
            }

            if (ModelState.IsValid)
            {
                // 3. Розрахунок різниці кількості
                int quantityDiff = orderItem.Quantity - originalItem.Quantity;

                var productToUpdate = db.Products.Find(originalItem.Offer.Product.Id);
                var orderToUpdate = db.Orders.Find(orderItem.OrderId);

                // 4. Перевірка складу (якщо збільшуємо кількість)
                if (quantityDiff > 0)
                {
                    if (productToUpdate.Stock < quantityDiff)
                    {
                        ModelState.AddModelError("Quantity", $"Недостатньо товару на складі. Доступно додатково: {productToUpdate.Stock}");

                        ViewBag.OrderId = new SelectList(db.Orders, "Id", "Description", orderItem.OrderId);
                        var offersList = db.SupplierOffers.Include(so => so.Product).Include(so => so.Supplier).ToList().Select(s => new { Id = s.Id, Text = $"{s.Product.Name} ({s.Supplier.Name}) - {s.Price:C}" });
                        ViewBag.SupplierOfferId = new SelectList(offersList, "Id", "Text", orderItem.SupplierOfferId);
                        return View(orderItem);
                    }
                }

                // 5. Оновлення складу
                productToUpdate.Stock -= quantityDiff;

                // 6. Перерахунок сум
                orderItem.Amount = orderItem.Quantity * orderItem.UnitPrice;
                decimal amountDiff = orderItem.Amount - originalItem.Amount;
                orderToUpdate.TotalAmount += amountDiff;

                // 7. Збереження
                db.Entry(productToUpdate).State = EntityState.Modified;
                db.Entry(orderToUpdate).State = EntityState.Modified;
                db.Entry(orderItem).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OrderId = new SelectList(db.Orders, "Id", "Description", orderItem.OrderId);
            var offersFinal = db.SupplierOffers.Include(so => so.Product).Include(so => so.Supplier).ToList().Select(s => new { Id = s.Id, Text = $"{s.Product.Name} ({s.Supplier.Name}) - {s.Price:C}" });
            ViewBag.SupplierOfferId = new SelectList(offersFinal, "Id", "Text", orderItem.SupplierOfferId);
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

            // При видаленні повертаємо товар на склад
            var product = db.Products.Find(orderItem.Offer.Product.Id);
            if (product != null)
            {
                product.Stock += orderItem.Quantity;
                db.Entry(product).State = EntityState.Modified;
            }

            // Зменшуємо суму замовлення
            var order = db.Orders.Find(orderItem.OrderId);
            if (order != null)
            {
                order.TotalAmount -= orderItem.Amount;
                db.Entry(order).State = EntityState.Modified;
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