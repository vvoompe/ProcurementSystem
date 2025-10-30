using ProcurementSystem.Models;
using ProcurementSystem.Models.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace ProcurementSystem.Data
{
    public class DbInitializer : DropCreateDatabaseIfModelChanges<ProcUREMENTContext>
    {
        protected override void Seed(ProcurementContext context)
        {
            var user1 = new User { Login = "admin_user", Password = "password", Role = UserRole.АДМІНІСТРАТОР };
            var user2 = new User { Login = "manager_user", Password = "password", Role = UserRole.МЕНЕДЖЕР };
            context.Users.Add(user1);
            context.Users.Add(user2);

            var category1 = new Category { Name = "Ноутбуки", Description = "Портативні комп'ютери" };
            var category2 = new Category { Name = "Принтери", Description = "Друкарські пристрої" };
            var category3 = new Category { Name = "Монітори", Description = "Екрани" };
            context.Categories.Add(category1);
            context.Categories.Add(category2);
            context.Categories.Add(category3);

            context.Products.Add(new Product { Name = "Dell XPS 15", Price = 45000, Stock = 10, Category = category1 });
            context.Products.Add(new Product { Name = "MacBook Pro 16", Price = 75000, Stock = 5, Category = category1 });
            context.Products.Add(new Product { Name = "HP LaserJet M110w", Price = 5200, Stock = 20, Category = category2 });
            context.Products.Add(new Product { Name = "Canon PIXMA G3420", Price = 7100, Stock = 15, Category = category2 });
            context.Products.Add(new Product { Name = "Samsung Odyssey G7", Price = 21000, Stock = 8, Category = category3 });
            context.Products.Add(new Product { Name = "Dell UltraSharp U2723QE", Price = 28000, Stock = 12, Category = category3 });
            context.Products.Add(new Product { Name = "Lenovo ThinkPad X1", Price = 52000, Stock = 7, Category = category1 });
            context.Products.Add(new Product { Name = "Epson L3251", Price = 8500, Stock = 18, Category = category2 });
            context.Products.Add(new Product { Name = "AOC 24G2U", Price = 6500, Stock = 25, Category = category3 });
            context.Products.Add(new Product { Name = "Asus ROG Zephyrus G14", Price = 61000, Stock = 9, Category = category1 });

            var supplier1 = new Supplier { Name = "Rozetka", Contacts = "044-537-02-22" };
            var supplier2 = new Supplier { Name = "Brain", Contacts = "0800-21-11-20" };
            context.Suppliers.Add(supplier1);
            context.Suppliers.Add(supplier2);

            context.SaveChanges();

            var order1 = new Order
            {
                OrderDate = new DateTime(2025, 10, 20),
                Status = OrderStatus.ДОСТАВЛЕНО,
                TotalAmount = 52100,
                User = user1
            };
            var order2 = new Order
            {
                OrderDate = new DateTime(2025, 10, 22),
                Status = OrderStatus.ВІДПРАВЛЕНО,
                TotalAmount = 75000,
                User = user2
            };
            context.Orders.Add(order1);
            context.Orders.Add(order2);

            base.Seed(context);
        }
    }
}