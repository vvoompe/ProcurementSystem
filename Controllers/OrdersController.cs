using ProcurementSystem.Data; // Підключіть ваш контекст
using ProcurementSystem.Models; // Підключіть ваші моделі
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity; // ДУЖЕ ВАЖЛИВО! Потрібно для .Include()

namespace ProcurementSystem.Controllers
{
    public class OrdersController : Controller
    {
        // 1. Створюємо екземпляр контексту
        private ProcurementContext db = new ProcurementContext();

        // 2. Редагуємо метод Index
        public ActionResult Index()
        {
            // Ми хочемо показувати не тільки ID користувача, 
            // а і його ім'я. Для цього використовуємо .Include()
            // Це каже EF: "Коли завантажуєш замовлення, 
            // будь ласка, одразу завантаж і пов'язаного з ним користувача".
            var orders = db.Orders.Include(o => o.User);

            return View(orders.ToList());
        }
        // GET: Orders/Create
        // Цей метод готує дані для форми створення
        public ActionResult Create()
        {
            // Згідно з нашою діаграмою класів, замовлення створює Користувач
            ViewBag.UserId = new SelectList(db.Users, "Id", "Login");

            // Тут має бути складніша логіка для додавання товарів, 
            // але для початку нам вистачить цього.

            // Встановлюємо початковий статус згідно з "Діаграмою станів"
            var newOrder = new Order
            {
                OrderDate = System.DateTime.Now,
                Status = Models.Enums.OrderStatus.ЧОРНЕТКА
            };

            return View(newOrder);
        }
    }
}