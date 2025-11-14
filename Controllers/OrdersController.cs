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
    public class OrdersController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: Orders
        public ActionResult Index()
        {
            IQueryable<Order> orders = db.Orders.Include(o => o.User);

            if (User.IsInRole("СПІВРОБІТНИК"))
            {
                string currentUserLogin = User.Identity.Name;
                var currentUser = db.Users.FirstOrDefault(u => u.Login == currentUserLogin);

                if (currentUser != null)
                {
                    orders = orders.Where(o => o.UserId == currentUser.Id);
                }
                else
                {
                    orders = orders.Where(o => false);
                }
            }

            return View(orders.ToList());
        }

        // GET: Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders
                            .Include(o => o.User)
                            .Include(o => o.OrderItems.Select(oi => oi.Offer.Product))
                            .Include(o => o.Invoices)
                            .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            if (User.IsInRole("СПІВРОБІТНИК"))
            {
                string currentUserLogin = User.Identity.Name;
                var currentUser = db.Users.FirstOrDefault(u => u.Login == currentUserLogin);
                if (currentUser == null || order.UserId != currentUser.Id)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Ви не можете переглядати чужі замовлення.");
                }
            }

            return View(order);
        }

        // GET: Orders/Create
        [Authorize(Roles = "СПІВРОБІТНИК")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "СПІВРОБІТНИК")]

        public ActionResult Create([Bind(Include = "TotalAmount, Description")] Order order)
        {
            string currentUserLogin = User.Identity.Name;
            var currentUser = db.Users.FirstOrDefault(u => u.Login == currentUserLogin);
            if (currentUser == null)
            {
                ModelState.AddModelError("", "Помилка автентифікації користувача.");
                return View(order);
            }

            order.UserId = currentUser.Id;
            order.OrderDate = DateTime.Now;
            order.Status = OrderStatus.ВІДПРАВЛЕНО;

            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                db.SaveChanges();

                return RedirectToAction("Details", new { id = order.Id });
            }

            return View(order);
        }

        // GET: Orders/Edit/5
        [Authorize(Roles = "МЕНЕДЖЕР, АДМІНІСТРАТОР")]
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
            ViewBag.UserId = new SelectList(db.Users, "Id", "Login", order.UserId);
            ViewBag.Status = new SelectList(
                Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id",
                "Name",
                (int)order.Status);
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "МЕНЕДЖЕР, АДМІНІСТРАТОР")]
        public ActionResult Edit([Bind(Include = "Id,OrderDate,Status,TotalAmount,UserId")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "Login", order.UserId);
            ViewBag.Status = new SelectList(
                Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id",
                "Name",
                (int)order.Status);
            return View(order);
        }

        // GET: Orders/Delete/5
        [Authorize(Roles = "МЕНЕДЖЕР, АДМІНІСТРАТОР")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Include(o => o.User).FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "МЕНЕДЖЕР, АДМІНІСТРАТОР")]
        public ActionResult DeleteConfirmed(int id)
        {
            bool hasOrderItems = db.OrderItems.Any(oi => oi.OrderId == id);
            bool hasInvoices = db.Invoices.Any(i => i.OrderId == id);
            bool hasReportOrders = db.ReportOrders.Any(ro => ro.OrderId == id);

            Order order = db.Orders.Find(id);

            if (hasOrderItems || hasInvoices || hasReportOrders)
            {
                string errorMessage = "Неможливо видалити замовлення. ";
                if (hasOrderItems) errorMessage += "Існують пов'язані позиції. ";
                if (hasInvoices) errorMessage += "Існують пов'язані рахунки. ";
                if (hasReportOrders) errorMessage += "Існують пов'язані звіти.";

                ModelState.AddModelError("", errorMessage);

                db.Entry(order).Reference(o => o.User).Load();
                return View(order);
            }

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