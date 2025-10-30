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
    public class ReportOrdersController : Controller
    {
        private ProcurementContext db = new ProcurementContext();
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

        private void PopulateReportsDropDownList(object selectedReport = null)
        {
            ViewBag.ReportId = new SelectList(db.Reports.OrderBy(r => r.Period), "Id", "Type", selectedReport);
        }


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
            PopulateOrdersDropDownList();
            PopulateReportsDropDownList();
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

            PopulateOrdersDropDownList(reportOrder.OrderId);
            PopulateReportsDropDownList(reportOrder.ReportId);
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
            PopulateOrdersDropDownList(reportOrder.OrderId);
            PopulateReportsDropDownList(reportOrder.ReportId);
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
            PopulateOrdersDropDownList(reportOrder.OrderId);
            PopulateReportsDropDownList(reportOrder.ReportId);
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
            ReportOrder reportOrder = db.ReportOrders.Find(id);
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