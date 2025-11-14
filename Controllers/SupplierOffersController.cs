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
    [Authorize(Roles = "МЕНЕДЖЕР, АДМІНІСТРАТОР")]
    public class SupplierOffersController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: SupplierOffers
        public ActionResult Index()
        {
            var supplierOffers = db.SupplierOffers.Include(s => s.Product).Include(s => s.Supplier);
            return View(supplierOffers.ToList());
        }

        // GET: SupplierOffers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SupplierOffer supplierOffer = db.SupplierOffers
                                            .Include(s => s.Product)
                                            .Include(s => s.Supplier)
                                            .FirstOrDefault(s => s.Id == id);
            if (supplierOffer == null)
            {
                return HttpNotFound();
            }
            return View(supplierOffer);
        }

        // GET: SupplierOffers/Create
        public ActionResult Create()
        {
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name");
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name");
            return View();
        }

        // POST: SupplierOffers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Price,SupplierId,ProductId")] SupplierOffer supplierOffer)
        {
            if (ModelState.IsValid)
            {
                db.SupplierOffers.Add(supplierOffer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", supplierOffer.ProductId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name", supplierOffer.SupplierId);
            return View(supplierOffer);
        }

        // GET: SupplierOffers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SupplierOffer supplierOffer = db.SupplierOffers.Find(id);
            if (supplierOffer == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", supplierOffer.ProductId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name", supplierOffer.SupplierId);
            return View(supplierOffer);
        }

        // POST: SupplierOffers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Price,SupplierId,ProductId")] SupplierOffer supplierOffer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(supplierOffer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", supplierOffer.ProductId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name", supplierOffer.SupplierId);
            return View(supplierOffer);
        }

        // GET: SupplierOffers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SupplierOffer supplierOffer = db.SupplierOffers
                                            .Include(s => s.Product)
                                            .Include(s => s.Supplier)
                                            .FirstOrDefault(s => s.Id == id);
            if (supplierOffer == null)
            {
                return HttpNotFound();
            }
            return View(supplierOffer);
        }

        // POST: SupplierOffers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            bool isLinked = db.OrderItems.Any(oi => oi.SupplierOfferId == id);
            SupplierOffer supplierOffer = db.SupplierOffers.Find(id);

            if (isLinked)
            {
                ModelState.AddModelError("", "Неможливо видалити пропозицію, оскільки вона використовується в існуючих замовленнях.");
                db.Entry(supplierOffer).Reference(s => s.Product).Load();
                db.Entry(supplierOffer).Reference(s => s.Supplier).Load();
                return View(supplierOffer);
            }

            db.SupplierOffers.Remove(supplierOffer);
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