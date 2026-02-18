using System.Diagnostics;
using System.IO;
using System.Web;
using ProcurementSystem.Models;

namespace ProcurementSystem.Patterns.Observer
{
    public class EmailNotificationObserver : IOrderObserver
    {
        public void Update(Order order)
        {
            string message = $"[EMAIL] Шановний {order.User.Login}, статус вашого замовлення #{order.Id} змінено на '{order.Status}'.";
            Debug.WriteLine(message);
        }
    }

    public class LogFileObserver : IOrderObserver
    {
        public void Update(Order order)
        {
            try
            {
                string logPath = HttpContext.Current.Server.MapPath("~/App_Data/order_log.txt");
                string logMessage = $"{System.DateTime.Now}: Замовлення #{order.Id} змінило статус на {order.Status}. Сума: {order.TotalAmount}";

                File.AppendAllText(logPath, logMessage + System.Environment.NewLine);
                Debug.WriteLine($"[LOG] Записано у файл: {logMessage}");
            }
            catch
            {
            }
        }
    }
}