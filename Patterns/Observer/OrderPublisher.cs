using System.Collections.Generic;
using ProcurementSystem.Models;
using ProcurementSystem.Models.Enums;

namespace ProcurementSystem.Patterns.Observer
{
    public class OrderStatusPublisher : IOrderSubject
    {
        private List<IOrderObserver> _observers = new List<IOrderObserver>();

        public void Attach(IOrderObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IOrderObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(Order order)
        {
            foreach (var observer in _observers)
            {
                observer.Update(order);
            }
        }

        public void ChangeStatus(Order order, OrderStatus newStatus)
        {
            if (order.Status != newStatus)
            {
                order.Status = newStatus;
                Notify(order);
            }
        }
    }
}