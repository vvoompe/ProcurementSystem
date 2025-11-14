using ProcurementSystem.Models;
using ProcurementSystem.ViewModels;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security; 

namespace ProcurementSystem.Controllers
{
    public class AccountController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = db.Users.FirstOrDefault(u => u.Login == model.Login && u.Password == model.Password);

            if (user != null)
            {
                FormsAuthentication.SetAuthCookie(user.Login, false);

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Products");
            }
            else
            {
                ModelState.AddModelError("", "Неправильний логін або пароль.");
            }

            return View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Products");
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