using ProcurementSystem.Models;
using ProcurementSystem.ViewModels; 
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System;

namespace ProcurementSystem.Controllers
{
    public class AccountController : Controller
    {
        private ProcurementContext db = new ProcurementContext();

        //
        // GET: /Account/Login
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loginNormalized = (model.Login ?? string.Empty).Trim().ToLower();
            var passwordTrimmed = (model.Password ?? string.Empty).Trim();

            var user = db.Users.FirstOrDefault(u => u.Login.ToLower() == loginNormalized && u.Password == passwordTrimmed);

            if (user != null)
            {
                var rolesString = user.Role.ToString();
                var ticket = new FormsAuthenticationTicket(
                    1,
                    user.Login,
                    DateTime.Now,
                    DateTime.Now.AddHours(8),
                    false,
                    rolesString
                );

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                var authCookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                {
                    HttpOnly = true,
                    Secure = FormsAuthentication.RequireSSL
                };
                Response.Cookies.Add(authCookie);

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

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut(); 
            return RedirectToAction("Index", "Products");
        }
    }
}