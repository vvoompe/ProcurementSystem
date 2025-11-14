using ProcurementSystem.Models;
using ProcurementSystem.Models.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ProcurementSystem.Data
{
    public class DbInitializer : DropCreateDatabaseIfModelChanges<ProcurementContext>
    {
        protected override void Seed(ProcurementContext context)
        {
            var users = new List<User>
            {
                new User { Login = "admin", Password = "admin_password", Role = UserRole.АДМІНІСТРАТОР },
                new User { Login = "manager", Password = "manager_password", Role = UserRole.МЕНЕДЖЕР },
                new User { Login = "employee", Password = "employee_password", Role = UserRole.СПІВРОБІТНИК },
                new User { Login = "accountant", Password = "accountant_password", Role = UserRole.БУХГАЛТЕР }
            };
            users.ForEach(u => context.Users.Add(u));
            context.SaveChanges();

            var categories = new List<Category>
            {
                new Category { Name = "Офісна Техніка", Description = "Принтери, сканери, МФУ" },
                new Category { Name = "Канцтовари", Description = "Папір, ручки, степлери" },
                new Category { Name = "Меблі", Description = "Столи, стільці" }
            };
            categories.ForEach(c => context.Categories.Add(c));
            context.SaveChanges();

            var products = new List<Product>
            {
                new Product { Name = "Принтер HP LaserJet", Description = "Чорно-білий принтер", Price = 4500.00m, Stock = 10, CategoryId = categories.Single(c => c.Name == "Офісна Техніка").Id },
                new Product { Name = "Папір A4 (500 арк.)", Description = "Стандартний офісний папір", Price = 150.00m, Stock = 100, CategoryId = categories.Single(c => c.Name == "Канцтовари").Id },
                new Product { Name = "Стілець офісний", Description = "Ергономічний стілець", Price = 2500.00m, Stock = 20, CategoryId = categories.Single(c => c.Name == "Меблі").Id }
            };
            products.ForEach(p => context.Products.Add(p));
            context.SaveChanges();

            var suppliers = new List<Supplier>
            {
                new Supplier { Name = "ТОВ 'ТехноСвіт'", Contacts = "info@techno.ua, +380441234567" },
                new Supplier { Name = "ТОВ 'КанцПром'", Contacts = "sales@kanz.ua, +380447654321" }
            };
            suppliers.ForEach(s => context.Suppliers.Add(s));
            context.SaveChanges();

            var supplierOffers = new List<SupplierOffer>
            {
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Принтер HP LaserJet").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'ТехноСвіт'").Id, Price = 4450.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Папір A4 (500 арк.)").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'КанцПром'").Id, Price = 145.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Стілець офісний").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'ТехноСвіт'").Id, Price = 2480.00m }
            };
            supplierOffers.ForEach(so => context.SupplierOffers.Add(so));
            context.SaveChanges();

            var orders = new List<Order>
            {
                new Order { OrderDate = DateTime.Now.AddDays(-10), Description = "Замовлення для бухгалтерії", Status = OrderStatus.ДОСТАВЛЕНО, TotalAmount = 145.00m, UserId = users.Single(u => u.Login == "employee").Id },
                new Order { OrderDate = DateTime.Now.AddDays(-5), Description = "Замовлення для відділу кадрів", Status = OrderStatus.ВІДПРАВЛЕНО, TotalAmount = 4450.00m, UserId = users.Single(u => u.Login == "employee").Id }
            };
            orders.ForEach(o => context.Orders.Add(o));
            context.SaveChanges();

            var orderItems = new List<OrderItem>
            {
                new OrderItem { Quantity = 1, UnitPrice = 145.00m, Amount = 145.00m, OrderId = orders.Single(o => o.Description == "Замовлення для бухгалтерії").Id, SupplierOfferId = supplierOffers.Single(so => so.Price == 145.00m).Id },
                new OrderItem { Quantity = 1, UnitPrice = 4450.00m, Amount = 4450.00m, OrderId = orders.Single(o => o.Description == "Замовлення для відділу кадрів").Id, SupplierOfferId = supplierOffers.Single(so => so.Price == 4450.00m).Id }
            };
            orderItems.ForEach(oi => context.OrderItems.Add(oi));
            context.SaveChanges();

            var invoices = new List<Invoice>
            {
                new Invoice { OrderId = orders.Single(o => o.Description == "Замовлення для бухгалтерії").Id, Amount = 145.00m, DueDate = DateTime.Now.AddDays(5), PaymentStatus = PaymentStatus.ОПЛАЧЕНО },
                new Invoice { OrderId = orders.Single(o => o.Description == "Замовлення для відділу кадрів").Id, Amount = 4450.00m, DueDate = DateTime.Now.AddDays(10), PaymentStatus = PaymentStatus.ОЧІКУЄТЬСЯ }
            };
            invoices.ForEach(i => context.Invoices.Add(i));
            context.SaveChanges();

        }
    }
}