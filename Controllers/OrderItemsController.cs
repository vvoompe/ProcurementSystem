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

namespace ProcurementSystem.Controllers
{
    public class OrderItemsController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        private void PopulateOffersDropDownList(object selectedOffer = null)
        {
            var offerList = db.SupplierOffers
                .Include(o => o.Product)
                .Include(o => o.Supplier)
                .OrderBy(o => o.Product.Name)
                .Select(o => new {
                    o.Id,
                    DisplayText = o.Product.Name + " (від " + o.Supplier.Name + ", " + o.Price + " грн)"
                }).ToList();

            ViewBag.SupplierOfferId = new SelectList(offerList, "Id", "DisplayText", selectedOffer);
        }
        private void PopulateOrdersDropDownList(object selectedOrder = null)
        {
            var orderList = db.Orders
               .Include(o => o.User)
               .OrderByDescending(o => o.OrderDate)
               .Select(o => new {
                   o.Id,
                   DisplayText = "Замовлення №" + o.Id + " (від " + o.User.Login + ")"
               }).ToList();

            ViewBag.OrderId = new SelectList(orderList, "Id", "DisplayText", selectedOrder);
        }


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
            PopulateOrdersDropDownList();
            PopulateOffersDropDownList();
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
                // TODO: Потрібно оновити TotalAmount в батьківському Order
                return RedirectToAction("Index");
            }

            PopulateOrdersDropDownList(orderItem.OrderId);
            PopulateOffersDropDownList(orderItem.SupplierOfferId);
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
            PopulateOrdersDropDownList(orderItem.OrderId);
            PopulateOffersDropDownList(orderItem.SupplierOfferId);
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
                // TODO: Потрібно оновити TotalAmount в батьківському Order
                return RedirectToAction("Index");
            }
            PopulateOrdersDropDownList(orderItem.OrderId);
            PopulateOffersDropDownList(orderItem.SupplierOfferId);
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
                                    .Include(o => o.Offer.Product)
                                    .Include(o => o.Order.User)
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
            OrderItem orderItem = db.OrderItems.Find(id);
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