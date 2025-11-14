using ProcurementSystem.Models;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security; // Важливо для автентифікації

namespace ProcurementSystem.Controllers
{
    public class AccountController : Controller
    {
        // Наш зв'язок з базою даних
        private ProcurementContext db = new ProcurementContext();

        //
        // GET: /Account/Login
        // Цей метод просто показує сторінку з формою логіну
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        // Цей метод спрацьовує, коли користувач натискає кнопку "Увійти"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Шукаємо користувача в базі даних
            var user = db.Users.FirstOrDefault(u => u.Login == model.Login && u.Password == model.Password);

            if (user != null)
            {
                // УСПІШНИЙ ЛОГІН
                // Ми створюємо "cookie" автентифікації.
                // Система запам'ятає користувача.
                FormsAuthentication.SetAuthCookie(user.Login, false); // false - не запам'ятовувати надовго

                // Якщо користувач намагався увійти на захищену сторінку,
                // повертаємо його туди.
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Інакше перенаправляємо на головну сторінку
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Якщо користувача не знайдено, показуємо помилку
                ModelState.AddModelError("", "Неправильний логін або пароль.");
            }

            return View(model);
        }

        //
        // POST: /Account/LogOff
        // Метод для виходу з системи
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut(); // Видаляємо cookie
            return RedirectToAction("Index", "Home");
        }
    }
}