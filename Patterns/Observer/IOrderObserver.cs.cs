using ProcurementSystem.Models;

namespace ProcurementSystem.Patterns.Observer
{
    public interface IOrderObserver
    {
        void Update(Order order);
    }

    public interface IOrderSubject
    {
        void Attach(IOrderObserver observer);
        void Detach(IOrderObserver observer);
        void Notify(Order order);
    }
}