using ProcurementSystem;
using ProcurementSystem.Models;
using ProcurementSystem.Models.Enums; // Потрібно для OrderStatus
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

                if (order.UserId == currentUser.Id && order.Status == OrderStatus.ВІДПРАВЛЕНО)
                {
                    return true;
                }
            }

            return false;
        }

        // GET: OrderItems
        [Authorize(Roles = "МЕНЕДЖЕР, АДМІНІСТРАТОР")]
        public ActionResult Index()
        {
            var orderItems = db.OrderItems.Include(o => o.Offer).Include(o => o.Order);
            return View(orderItems.ToList());
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

            ViewBag.OfferId = new SelectList(db.SupplierOffers, "Id", "Id"); // Краще показувати ім'я товару
            var model = new OrderItem { OrderId = orderId.Value };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,OrderId,OfferId,Quantity,UnitPrice")] OrderItem orderItem)
        {
            Order order;
            if (!HasPermission(orderItem.OrderId, out order))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Ви не можете додавати позиції до цієї заявки.");
            }

            if (ModelState.IsValid)
            {
                db.OrderItems.Add(orderItem);
                db.SaveChanges();
                return RedirectToAction("Details", "Orders", new { id = orderItem.OrderId });
            }

            ViewBag.OfferId = new SelectList(db.SupplierOffers, "Id", "Id", orderItem.SupplierOfferId);
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

            ViewBag.OfferId = new SelectList(db.SupplierOffers, "Id", "Id", orderItem.SupplierOfferId);
            return View(orderItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,OrderId,OfferId,Quantity,UnitPrice")] OrderItem orderItem)
        {
            Order order;
            if (!HasPermission(orderItem.OrderId, out order))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Ви не можете редагувати цю позицію.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(orderItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Orders", new { id = orderItem.OrderId });
            }
            ViewBag.OfferId = new SelectList(db.SupplierOffers, "Id", "Id", orderItem.SupplierOfferId);
            return View(orderItem);
        }


        // GET: OrderItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderItem orderItem = db.OrderItems.Include(oi => oi.Order).FirstOrDefault(oi => oi.Id == id);
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