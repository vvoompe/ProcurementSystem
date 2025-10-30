using ProcurementSystem.Models;
using ProcurementSystem.Models.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace ProcurementSystem.Data
{
    public class DbInitializer : DropCreateDatabaseIfModelChanges<ProcurementContext>
    {
        protected override void Seed(ProcurementContext context)
        {
            // 1. Користувачі
            var user1 = new User { Login = "admin_user", Password = "password", Role = UserRole.АДМІНІСТРАТОР };
            var user2 = new User { Login = "manager_user", Password = "password", Role = UserRole.МЕНЕДЖЕР };
            context.Users.Add(user1);
            context.Users.Add(user2);

            // 2. Категорії
            var category1 = new Category { Name = "Ноутбуки", Description = "Портативні комп'ютери" };
            var category2 = new Category { Name = "Принтери", Description = "Друкарські пристрої" };
            var category3 = new Category { Name = "Монітори", Description = "Екрани" };
            context.Categories.Add(category1);
            context.Categories.Add(category2);
            context.Categories.Add(category3);

            // 3. Товари
            var product1 = new Product { Name = "Dell XPS 15", Price = 45000, Stock = 10, Category = category1 };
            var product2 = new Product { Name = "MacBook Pro 16", Price = 75000, Stock = 5, Category = category1 };
            var product3 = new Product { Name = "HP LaserJet M110w", Price = 5200, Stock = 20, Category = category2 };
            var product4 = new Product { Name = "Canon PIXMA G3420", Price = 7100, Stock = 15, Category = category2 };
            var product5 = new Product { Name = "Samsung Odyssey G7", Price = 21000, Stock = 8, Category = category3 };
            var product6 = new Product { Name = "Dell UltraSharp U2723QE", Price = 28000, Stock = 12, Category = category3 };
            var product7 = new Product { Name = "Lenovo ThinkPad X1", Price = 52000, Stock = 7, Category = category1 };
            var product8 = new Product { Name = "Epson L3251", Price = 8500, Stock = 18, Category = category2 };
            var product9 = new Product { Name = "AOC 24G2U", Price = 6500, Stock = 25, Category = category3 };
            var product10 = new Product { Name = "Asus ROG Zephyrus G14", Price = 61000, Stock = 9, Category = category1 };
            context.Products.AddRange(new List<Product>
            {
                product1, product2, product3, product4, product5, product6, product7, product8, product9, product10
            });

            // 4. Постачальники
            var supplier1 = new Supplier { Name = "Rozetka", Contacts = "044-537-02-22" };
            var supplier2 = new Supplier { Name = "Brain", Contacts = "0800-21-11-20" };
            context.Suppliers.Add(supplier1);
            context.Suppliers.Add(supplier2);

            // 5. Замовлення
            var order1 = new Order
            {
                OrderDate = new DateTime(2025, 10, 20),
                Status = OrderStatus.ДОСТАВЛЕНО,
                TotalAmount = 52200,
                User = user1
            };
            var order2 = new Order
            {
                OrderDate = new DateTime(2025, 10, 22),
                Status = OrderStatus.ВІДПРАВЛЕНО,
                TotalAmount = 74500,
                User = user2
            };
            context.Orders.Add(order1);
            context.Orders.Add(order2);

            var offer1 = new SupplierOffer { Price = 45000, Supplier = supplier1, Product = product1 };
            var offer2 = new SupplierOffer { Price = 7200, Supplier = supplier2, Product = product4 };
            var offer3 = new SupplierOffer { Price = 74500, Supplier = supplier1, Product = product2 };
            var offer4 = new SupplierOffer { Price = 5100, Supplier = supplier2, Product = product3 };
            context.SupplierOffers.AddRange(new List<SupplierOffer> { offer1, offer2, offer3, offer4 });

            var item1 = new OrderItem { Quantity = 1, UnitPrice = 45000, Amount = 45000, Order = order1, Offer = offer1 };
            var item2 = new OrderItem { Quantity = 1, UnitPrice = 7200, Amount = 7200, Order = order1, Offer = offer2 };
            var item3 = new OrderItem { Quantity = 1, UnitPrice = 74500, Amount = 74500, Order = order2, Offer = offer3 };
            context.OrderItems.AddRange(new List<OrderItem> { item1, item2, item3 });

            var invoice1 = new Invoice { Amount = 52200, InvoiceDate = new DateTime(2025, 10, 21), Status = PaymentStatus.ОПЛАЧЕНО, Order = order1 };
            var invoice2 = new Invoice { Amount = 74500, InvoiceDate = new DateTime(2025, 10, 23), Status = PaymentStatus.ОЧІКУЄТЬСЯ, Order = order2 };
            context.Invoices.AddRange(new List<Invoice> { invoice1, invoice2 });

            var report1 = new Report { Period = "Q4 2025", Type = "Квартальний огляд закупівель" };
            context.Reports.Add(report1);

            var ro1 = new ReportOrder { GenerationDate = new DateTime(2025, 11, 1), Report = report1, Order = order1 };
            var ro2 = new ReportOrder { GenerationDate = new DateTime(2025, 11, 1), Report = report1, Order = order2 };
            context.ReportOrders.AddRange(new List<ReportOrder> { ro1, ro2 });

            context.SaveChanges();

            base.Seed(context);
        }
    }
}