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
using ProcurementSystem.ViewModels;

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

            // Ми також додали бухгалтера до перегляду
            if (User.IsInRole("БУХГАЛТЕР"))
            {
                string currentUserLogin = User.Identity.Name;
                var currentUser = db.Users.FirstOrDefault(u => u.Login == currentUserLogin);

                if (currentUser == null)
                {
                    orders = orders.Where(o => false);
                }
                // Бухгалтер бачить всі замовлення, тому додатковий фільтр не потрібен
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
                            .Include(o => o.OrderItems.Select(oi => oi.Offer.Product.Category))
                            .Include(o => o.OrderItems.Select(oi => oi.Offer.Supplier))
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

            // Додано для логіки кнопки генерації рахунку
            ViewBag.CanGenerateInvoice = !order.Invoices.Any() &&
                                         (User.IsInRole("БУХГАЛТЕР") || User.IsInRole("АДМІНІСТРАТОР"));


            return View(order);
        }

        // GET: Orders/Create
        [Authorize(Roles = "СПІВРОБІТНИК")]
        public ActionResult Create()
        {
            var viewModel = new CreateOrderViewModel();

            var offers = db.SupplierOffers
                            .Include(so => so.Product)
                            .Include(so => so.Supplier)
                            .ToList();


            var offerSelectList = offers.AsEnumerable().Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Product.Name} (Пост: {p.Supplier.Name}) - {p.Price:C}"
            });

            viewModel.OfferList = new SelectList(offerSelectList, "Value", "Text");

            return View(viewModel);
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "СПІВРОБІТНИК")]
        public ActionResult Create(CreateOrderViewModel viewModel)
        {
            string currentUserLogin = User.Identity.Name;
            var currentUser = db.Users.FirstOrDefault(u => u.Login == currentUserLogin);

            if (currentUser == null)
            {
                ModelState.AddModelError("", "Помилка автентифікації користувача.");
            }

            var selectedOffer = db.SupplierOffers
                                  .Include(so => so.Product)
                                  .FirstOrDefault(so => so.Id == viewModel.SupplierOfferId);

            if (selectedOffer == null)
            {
                ModelState.AddModelError("SupplierOfferId", "Обраний товар не знайдено.");
            }
            else
            {
                if (selectedOffer.Product.Stock < viewModel.Quantity)
                {
                    ModelState.AddModelError("Quantity", $"Неможливо замовити більше, ніж є на складі. Доступно: {selectedOffer.Product.Stock}");
                }
            }

            if (ModelState.IsValid)
            {
                Order order = new Order();
                order.UserId = currentUser.Id;
                order.Description = viewModel.Description;
                order.OrderDate = DateTime.Now;
                order.Status = OrderStatus.ВІДПРАВЛЕНО;
                order.TotalAmount = selectedOffer.Price * viewModel.Quantity;
                db.Orders.Add(order);

                OrderItem orderItem = new OrderItem();
                orderItem.OrderId = order.Id;
                orderItem.SupplierOfferId = viewModel.SupplierOfferId;
                orderItem.Quantity = viewModel.Quantity;
                orderItem.UnitPrice = selectedOffer.Price;
                orderItem.Amount = selectedOffer.Price * viewModel.Quantity;
                db.OrderItems.Add(orderItem);

                var productToUpdate = db.Products.Find(selectedOffer.Product.Id);
                productToUpdate.Stock -= viewModel.Quantity;
                db.Entry(productToUpdate).State = EntityState.Modified;

                db.SaveChanges();

                return RedirectToAction("Details", new { id = order.Id });
            }

            var offers = db.SupplierOffers
                            .Include(so => so.Product)
                            .Include(so => so.Supplier)
                            .ToList();
            var offerSelectList = offers.AsEnumerable().Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Product.Name} (Пост: {p.Supplier.Name}) - {p.Price:C}"
            });

            viewModel.OfferList = new SelectList(offerSelectList, "Value", "Text", viewModel.SupplierOfferId);

            return View(viewModel);
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
        public ActionResult Edit([Bind(Include = "Id,OrderDate,Status,UserId,Description")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.Entry(order).Property(o => o.TotalAmount).IsModified = false;
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
            // bool hasReportOrders = db.ReportOrders.Any(ro => ro.OrderId == id); <-- ВИДАЛЕНО

            Order order = db.Orders.Find(id);

            // if (hasOrderItems || hasInvoices || hasReportOrders) <-- ЗМІНЕНО
            if (hasOrderItems || hasInvoices)
            {
                string errorMessage = "Неможливо видалити замовлення. ";
                if (hasOrderItems) errorMessage += "Існують пов'язані позиції. ";
                if (hasInvoices) errorMessage += "Існують пов'язані рахунки. ";
                // if (hasReportOrders) errorMessage += "Існують пов'язані звіти."; <-- ВИДАЛЕНО

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