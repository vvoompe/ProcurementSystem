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
    [Authorize(Roles = "БУХГАЛТЕР, АДМІНІСТРАТОР")]
    public class InvoicesController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: Invoices
        public ActionResult Index()
        {
            var invoices = db.Invoices.Include(i => i.Order.User);
            return View(invoices.ToList());
        }

        // GET: Invoices/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Include(i => i.Order.User).FirstOrDefault(i => i.Id == id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // POST: Invoices/GenerateInvoice
        // Цей метод генерує рахунок з Order/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateInvoice(int orderId)
        {
            bool invoiceExists = db.Invoices.Any(i => i.OrderId == orderId);
            if (invoiceExists)
            {
                TempData["ErrorMessage"] = "Рахунок для цього замовлення вже існує.";
                return RedirectToAction("Details", "Orders", new { id = orderId });
            }

            var order = db.Orders.Find(orderId);
            if (order == null)
            {
                return HttpNotFound("Замовлення не знайдено.");
            }

            Invoice invoice = new Invoice
            {
                OrderId = order.Id,
                Amount = order.TotalAmount, // Автоматичне копіювання
                DueDate = DateTime.Now.AddDays(14), // Термін 14 днів
                PaymentStatus = PaymentStatus.ОЧІКУЄТЬСЯ
            };

            db.Invoices.Add(invoice);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = invoice.Id });
        }


        // GET: Invoices/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Include(i => i.Order).FirstOrDefault(i => i.Id == id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            ViewBag.PaymentStatus = new SelectList(
                Enum.GetValues(typeof(PaymentStatus)).Cast<PaymentStatus>().Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id",
                "Name",
                (int)invoice.PaymentStatus);
            return View(invoice);
        }

        // POST: Invoices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Тільки ці поля можна редагувати
        public ActionResult Edit([Bind(Include = "Id,DueDate,PaymentStatus")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                var invoiceInDb = db.Invoices.Find(invoice.Id);
                if (invoiceInDb == null)
                {
                    return HttpNotFound();
                }

                invoiceInDb.DueDate = invoice.DueDate;
                invoiceInDb.PaymentStatus = invoice.PaymentStatus;

                db.Entry(invoiceInDb).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var originalInvoice = db.Invoices.Include(i => i.Order).FirstOrDefault(i => i.Id == invoice.Id);
            ViewBag.PaymentStatus = new SelectList(
                Enum.GetValues(typeof(PaymentStatus)).Cast<PaymentStatus>().Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id",
                "Name",
                (int)invoice.PaymentStatus);

            return View(originalInvoice);
        }


        // GET: Invoices/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Include(i => i.Order.User).FirstOrDefault(i => i.Id == id);
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