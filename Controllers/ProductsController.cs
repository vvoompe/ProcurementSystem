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
    public class ProductsController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: Products
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products
                                .Include(p => p.Category)
                                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,Price,Stock,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                bool productExists = db.Products.Any(p => p.Name == product.Name && p.Price == product.Price);
                if (productExists)
                {
                    ModelState.AddModelError("", "Товар з такою назвою та ціною вже існує.");
                    ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
                    return View(product);
                }

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Price,Stock,CategoryId")] Product product)
        {
            var offerIds = db.SupplierOffers
                             .Where(so => so.ProductId == product.Id)
                             .Select(so => so.Id);
            bool isUsedInOrders = db.OrderItems.Any(oi => offerIds.Contains(oi.SupplierOfferId));

            if (isUsedInOrders)
            {
                ModelState.AddModelError("", "Неможливо редагувати товар, оскільки він вже присутній у одному чи декількох замовленнях.");
                ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
                return View(product);
            }

            if (ModelState.IsValid)
            {
                bool productExists = db.Products.Any(p => p.Name == product.Name && p.Price == product.Price && p.Id != product.Id);
                if (productExists)
                {
                    ModelState.AddModelError("", "Інший товар з такою назвою та ціною вже існує.");
                    ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
                    return View(product);
                }

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "МЕНЕДЖЕР, АДМІНІСТРАТОР")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "МЕНЕДЖЕР, АДМІНІСТРАТОР")]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            var offerIds = db.SupplierOffers
                             .Where(so => so.ProductId == id)
                             .Select(so => so.Id);

            bool isUsedInOrders = db.OrderItems.Any(oi => offerIds.Contains(oi.SupplierOfferId));

            if (isUsedInOrders)
            {
                ModelState.AddModelError("", "Неможливо видалити товар, оскільки він вже присутній у одному чи декількох замовленнях.");
                db.Entry(product).Reference(p => p.Category).Load();
                return View(product);
            }

            var unusedOffers = db.SupplierOffers.Where(so => so.ProductId == id).ToList();
            if (unusedOffers.Any())
            {
                db.SupplierOffers.RemoveRange(unusedOffers);
            }

            db.Products.Remove(product);
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