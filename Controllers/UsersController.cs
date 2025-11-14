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
    [Authorize(Roles = "АДМІНІСТРАТОР")]
    public class UsersController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: Users
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.Role = new SelectList(
                Enum.GetValues(typeof(UserRole))
                    .Cast<UserRole>()
                    .Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id",
                "Name");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Login,Password,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Role = new SelectList(
                Enum.GetValues(typeof(UserRole))
                    .Cast<UserRole>()
                    .Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id",
                "Name",
                (int)user.Role); 

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.Role = new SelectList(
                Enum.GetValues(typeof(UserRole))
                    .Cast<UserRole>()
                    .Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id",
                "Name",
                (int)user.Role); 

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Login,Password,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Role = new SelectList(
                Enum.GetValues(typeof(UserRole))
                    .Cast<UserRole>()
                    .Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id",
                "Name",
                (int)user.Role);

            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            bool hasOrders = db.Orders.Any(o => o.UserId == id);

            User user = db.Users.Find(id);

            if (hasOrders)
            {
                ModelState.AddModelError("", "Неможливо видалити користувача, оскільки за ним закріплені заявки.");
                return View(user); 
            }

            db.Users.Remove(user);
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