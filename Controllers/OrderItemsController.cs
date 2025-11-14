using ProcurementSystem;
using ProcurementSystem.Models;
using ProcurementSystem.Models.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ProcurementSystem.Controllers
{
    [Authorize] 
    public class OrderItemsController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        private void UpdateOrderTotalAmount(int orderId)
        {
            var order = db.Orders.Find(orderId);
            if (order != null)
            {
                decimal newTotalAmount = db.OrderItems
                                           .Where(oi => oi.OrderId == orderId)
                                           .Sum(oi => (decimal?)oi.Amount) ?? 0; 

                order.TotalAmount = newTotalAmount;
                db.Entry(order).State = EntityState.Modified;
            }
        }

        private bool HasPermission(int orderId, out Order order)
        {
            order = db.Orders.Find(orderId);
            if (order == null)
            {
                return false;
            }

            if (User.IsInRole("МЕНЕДЖЕР") || User.IsInRole("АДМІНІСТРАТОР"))
            {
                return true;
            }

            if (User.IsInRole("СПІВРОБІТНИК"))
            {
                string currentUserLogin = User.Identity.Name;
                var currentUser = db.Users.FirstOrDefault(u => u.Login == currentUserLogin);

                if (currentUser != null && order.UserId == currentUser.Id && order.Status == OrderStatus.ВІДПРАВЛЕНО)
                {
                    return true;
                }
            }
            return false;
        }

        // GET: OrderItems/Create
        public ActionResult Create(int? orderId)
        {
            if (orderId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "OrderId не надано.");
            }

            Order order;
            if (!HasPermission(orderId.Value, out order))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Ви не можете додавати позиції до цієї заявки.");
            }

            // Заповнюємо ViewBag для випадаючого списку
            ViewBag.SupplierOfferId = new SelectList(
                db.SupplierOffers.Include(so => so.Product).Include(so => so.Supplier).AsEnumerable().Select(p => new {
                    Id = p.Id,
                    Text = $"{p.Product.Name} ({p.Supplier.Name}) - {p.Price:C}"
                }),
                "Id", "Text");

            var model = new OrderItem { OrderId = orderId.Value };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderId,SupplierOfferId,Quantity")] OrderItem orderItem)
        {
            Order order;
            if (!HasPermission(orderItem.OrderId, out order))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Ви не можете додавати позиції до цієї заявки.");
            }

            var selectedOffer = db.SupplierOffers.Find(orderItem.SupplierOfferId);
            if (selectedOffer == null)
            {
                ModelState.AddModelError("SupplierOfferId", "Обраний товар не знайдено.");
            }

            if (ModelState.IsValid)
            {
                orderItem.UnitPrice = selectedOffer.Price;
                orderItem.Amount = orderItem.Quantity * orderItem.UnitPrice;

                db.OrderItems.Add(orderItem);

                UpdateOrderTotalAmount(orderItem.OrderId);

                db.SaveChanges(); 

                return RedirectToAction("Details", "Orders", new { id = orderItem.OrderId });
            }

            ViewBag.SupplierOfferId = new SelectList(
                db.SupplierOffers.Include(so => so.Product).Include(so => so.Supplier).AsEnumerable().Select(p => new {
                    Id = p.Id,
                    Text = $"{p.Product.Name} ({p.Supplier.Name}) - {p.Price:C}"
                }),
                "Id", "Text", orderItem.SupplierOfferId);

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

            Order order;
            if (!HasPermission(orderItem.OrderId, out order))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Ви не можете редагувати цю позицію.");
            }

            ViewBag.SupplierOfferId = new SelectList(
                db.SupplierOffers.Include(so => so.Product).Include(so => so.Supplier).AsEnumerable().Select(p => new {
                    Id = p.Id,
                    Text = $"{p.Product.Name} ({p.Supplier.Name}) - {p.Price:C}"
                }),
                "Id", "Text", orderItem.SupplierOfferId);

            return View(orderItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,OrderId,SupplierOfferId,Quantity")] OrderItem orderItem)
        {
            Order order;
            if (!HasPermission(orderItem.OrderId, out order))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Ви не можете редагувати цю позицію.");
            }

            var selectedOffer = db.SupplierOffers.Find(orderItem.SupplierOfferId);
            if (selectedOffer == null)
            {
                ModelState.AddModelError("SupplierOfferId", "Обраний товар не знайдено.");
            }

            if (ModelState.IsValid)
            {
                orderItem.UnitPrice = selectedOffer.Price;
                orderItem.Amount = orderItem.Quantity * orderItem.UnitPrice;

                db.Entry(orderItem).State = EntityState.Modified;

                UpdateOrderTotalAmount(orderItem.OrderId);

                db.SaveChanges(); 

                return RedirectToAction("Details", "Orders", new { id = orderItem.OrderId });
            }

            ViewBag.SupplierOfferId = new SelectList(
                db.SupplierOffers.Include(so => so.Product).Include(so => so.Supplier).AsEnumerable().Select(p => new {
                    Id = p.Id,
                    Text = $"{p.Product.Name} ({p.Supplier.Name}) - {p.Price:C}"
                }),
                "Id", "Text", orderItem.SupplierOfferId);

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
                .Include(oi => oi.Order)
                .Include(oi => oi.Offer.Product)
                .FirstOrDefault(oi => oi.Id == id);

            if (orderItem == null)
            {
                return HttpNotFound();
            }

            Order order;
            if (!HasPermission(orderItem.OrderId, out order))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Ви не можете видаляти цю позицію.");
            }

            return View(orderItem);
        }

        // POST: OrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OrderItem orderItem = db.OrderItems.Find(id);
            if (orderItem == null)
            {
                return HttpNotFound();
            }

            Order order;
            if (!HasPermission(orderItem.OrderId, out order))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Ви не можете видаляти цю позицію.");
            }

            int orderId = orderItem.OrderId; 

            db.OrderItems.Remove(orderItem);

            UpdateOrderTotalAmount(orderId);

            db.SaveChanges(); 

            return RedirectToAction("Details", "Orders", new { id = orderId });
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