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
    public class ReportOrdersController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: ReportOrders
        public ActionResult Index()
        {
            var reportOrders = db.ReportOrders.Include(r => r.Order.User).Include(r => r.Report);
            return View(reportOrders.ToList());
        }

        // GET: ReportOrders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReportOrder reportOrder = db.ReportOrders
                                        .Include(r => r.Order.User)
                                        .Include(r => r.Report)
                                        .FirstOrDefault(r => r.Id == id);
            if (reportOrder == null)
            {
                return HttpNotFound();
            }
            return View(reportOrder);
        }

        // GET: ReportOrders/Create
        public ActionResult Create()
        {
            // 4. Покращено: Інформативні випадаючі списки
            ViewBag.OrderId = new SelectList(db.Orders.Include(o => o.User).AsEnumerable()
                .Select(o => new { Id = o.Id, Name = $"Заявка №{o.Id} ({o.User?.Login ?? "N/A"})" }), "Id", "Name");
            ViewBag.ReportId = new SelectList(db.Reports, "Id", "Period");
            return View();
        }

        // POST: ReportOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,GenerationDate,ReportId,OrderId")] ReportOrder reportOrder)
        {
            if (ModelState.IsValid)
            {
                db.ReportOrders.Add(reportOrder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OrderId = new SelectList(db.Orders.Include(o => o.User).AsEnumerable()
                .Select(o => new { Id = o.Id, Name = $"Заявка №{o.Id} ({o.User?.Login ?? "N/A"})" }), "Id", "Name", reportOrder.OrderId);
            ViewBag.ReportId = new SelectList(db.Reports, "Id", "Period", reportOrder.ReportId);
            return View(reportOrder);
        }

        // GET: ReportOrders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReportOrder reportOrder = db.ReportOrders.Find(id);
            if (reportOrder == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrderId = new SelectList(db.Orders.Include(o => o.User).AsEnumerable()
                .Select(o => new { Id = o.Id, Name = $"Заявка №{o.Id} ({o.User?.Login ?? "N/A"})" }), "Id", "Name", reportOrder.OrderId);
            ViewBag.ReportId = new SelectList(db.Reports, "Id", "Period", reportOrder.ReportId);
            return View(reportOrder);
        }

        // POST: ReportOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,GenerationDate,ReportId,OrderId")] ReportOrder reportOrder)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reportOrder).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OrderId = new SelectList(db.Orders.Include(o => o.User).AsEnumerable()
                .Select(o => new { Id = o.Id, Name = $"Заявка №{o.Id} ({o.User?.Login ?? "N/A"})" }), "Id", "Name", reportOrder.OrderId);
            ViewBag.ReportId = new SelectList(db.Reports, "Id", "Period", reportOrder.ReportId);
            return View(reportOrder);
        }

        // GET: ReportOrders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReportOrder reportOrder = db.ReportOrders
                                        .Include(r => r.Order.User)
                                        .Include(r => r.Report)
                                        .FirstOrDefault(r => r.Id == id);
            if (reportOrder == null)
            {
                return HttpNotFound();
            }
            return View(reportOrder);
        }

        // POST: ReportOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ReportOrder reportOrder = db.ReportOrders
                                        .Include(ro => ro.Order)
                                        .FirstOrDefault(ro => ro.Id == id);

            if (reportOrder == null)
            {
                return HttpNotFound();
            }


            if (reportOrder.Order != null &&
               (reportOrder.Order.Status == OrderStatus.ДОСТАВЛЕНО ||
                reportOrder.Order.Status == OrderStatus.СКАСОВАНО))
            {
                ModelState.AddModelError("", $"Неможливо видалити зв'язок: заявка №{reportOrder.OrderId} вже має фінальний статус '{reportOrder.Order.Status}'.");

                db.Entry(reportOrder).Reference(ro => ro.Report).Load();
                return View(reportOrder); 
            }

            db.ReportOrders.Remove(reportOrder);
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