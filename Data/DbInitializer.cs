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
            // 1. Користувачі (НЕ ЗМІНЕНО)
            var users = new List<User>
            {
                new User { Login = "admin", Password = "admin_password", Role = UserRole.АДМІНІСТРАТОР },
                new User { Login = "manager", Password = "manager_password", Role = UserRole.МЕНЕДЖЕР },
                new User { Login = "employee", Password = "employee_password", Role = UserRole.СПІВРОБІТНИК },
                new User { Login = "accountant", Password = "accountant_password", Role = UserRole.БУХГАЛТЕР }
            };
            users.ForEach(u => context.Users.Add(u));
            context.SaveChanges();

            // 2. Категорії
            var categories = new List<Category>
            {
                new Category { Name = "Офісна Техніка", Description = "Принтери, сканери, МФУ" },
                new Category { Name = "Канцтовари", Description = "Папір, ручки, степлери" },
                new Category { Name = "Меблі", Description = "Столи, стільці, шафи" },
                new Category { Name = "Комп'ютерні Аксесуари", Description = "Миші, клавіатури, кабелі" },
                new Category { Name = "Господарські Товари", Description = "Миючі засоби, рушники" },
                new Category { Name = "Кухонне Приладдя", Description = "Чайники, кавомашини, посуд" }
            };
            categories.ForEach(c => context.Categories.Add(c));
            context.SaveChanges();

            // 3. Постачальники
            var suppliers = new List<Supplier>
            {
                new Supplier { Name = "ТОВ 'ТехноСвіт'", Contacts = "info@techno.ua, +380441234567" },
                new Supplier { Name = "ТОВ 'КанцПром'", Contacts = "sales@kanz.ua, +380447654321" },
                new Supplier { Name = "ТОВ 'Меблі-Плюс'", Contacts = "mebli@plus.com, +380501112233" },
                new Supplier { Name = "ФОП 'Все-до-офісу'", Contacts = "vse@office.ua, +380674455667" },
                new Supplier { Name = "ТОВ 'ІТ-Компоненти'", Contacts = "it@comp.com, +380998877665" }
            };
            suppliers.ForEach(s => context.Suppliers.Add(s));
            context.SaveChanges();

            // 4. Товари
            var products = new List<Product>
            {
                // Офісна Техніка
                new Product { Name = "Принтер HP LaserJet", Description = "Чорно-білий принтер", Price = 4500.00m, Stock = 10, CategoryId = categories.Single(c => c.Name == "Офісна Техніка").Id },
                new Product { Name = "Сканер Canon", Description = "Планшетний сканер A4", Price = 2800.00m, Stock = 5, CategoryId = categories.Single(c => c.Name == "Офісна Техніка").Id },
                new Product { Name = "Монітор 27'' Dell", Description = "FullHD монітор", Price = 7200.00m, Stock = 8, CategoryId = categories.Single(c => c.Name == "Офісна Техніка").Id },
                
                // Канцтовари
                new Product { Name = "Папір A4 (500 арк.)", Description = "Стандартний офісний папір", Price = 150.00m, Stock = 100, CategoryId = categories.Single(c => c.Name == "Канцтовари").Id },
                new Product { Name = "Степлер (великий)", Description = "Пробиває до 50 аркушів", Price = 320.00m, Stock = 30, CategoryId = categories.Single(c => c.Name == "Канцтовари").Id },
                new Product { Name = "Набір ручок (10 шт, сині)", Description = "Кулькові ручки", Price = 80.00m, Stock = 50, CategoryId = categories.Single(c => c.Name == "Канцтовари").Id },

                // Меблі
                new Product { Name = "Стілець офісний", Description = "Ергономічний стілець", Price = 2500.00m, Stock = 20, CategoryId = categories.Single(c => c.Name == "Меблі").Id },
                new Product { Name = "Стіл робочий (L-подібний)", Description = "Стіл 160х120", Price = 4800.00m, Stock = 7, CategoryId = categories.Single(c => c.Name == "Меблі").Id },
                new Product { Name = "Шафа для документів", Description = "Висока, з полицями", Price = 3100.00m, Stock = 4, CategoryId = categories.Single(c => c.Name == "Меблі").Id },
                
                // Комп'ютерні Аксесуари
                new Product { Name = "Миша Logitech MX", Description = "Безпровідна лазерна миша", Price = 2100.00m, Stock = 15, CategoryId = categories.Single(c => c.Name == "Комп'ютерні Аксесуари").Id },
                new Product { Name = "Клавіатура K380", Description = "Безпровідна, компактна", Price = 1500.00m, Stock = 12, CategoryId = categories.Single(c => c.Name == "Комп'ютерні Аксесуари").Id },
                new Product { Name = "USB-Хаб", Description = "4 порти USB 3.0", Price = 450.00m, Stock = 25, CategoryId = categories.Single(c => c.Name == "Комп'ютерні Аксесуари").Id },
                
                // Господарські Товари
                new Product { Name = "Мило рідке (5л)", Description = "Антибактеріальне", Price = 250.00m, Stock = 40, CategoryId = categories.Single(c => c.Name == "Господарські Товари").Id },
                new Product { Name = "Рушники паперові (10 уп.)", Description = "Двошарові", Price = 300.00m, Stock = 30, CategoryId = categories.Single(c => c.Name == "Господарські Товари").Id },
                
                // Кухонне Приладдя
                new Product { Name = "Кавомашина Philips", Description = "Автоматична", Price = 11500.00m, Stock = 3, CategoryId = categories.Single(c => c.Name == "Кухонне Приладдя").Id },
                new Product { Name = "Чайник електричний", Description = "1.7л, нержавіюча сталь", Price = 800.00m, Stock = 10, CategoryId = categories.Single(c => c.Name == "Кухонне Приладдя").Id }
            };
            products.ForEach(p => context.Products.Add(p));
            context.SaveChanges();

            // 5. Пропозиції Постачальників
            var supplierOffers = new List<SupplierOffer>
            {
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Принтер HP LaserJet").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'ТехноСвіт'").Id, Price = 4450.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Принтер HP LaserJet").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'ІТ-Компоненти'").Id, Price = 4500.00m }, // Інша ціна
                
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Папір A4 (500 арк.)").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'КанцПром'").Id, Price = 145.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Папір A4 (500 арк.)").Id, SupplierId = suppliers.Single(s => s.Name == "ФОП 'Все-до-офісу'").Id, Price = 148.00m },

                new SupplierOffer { ProductId = products.Single(p => p.Name == "Стілець офісний").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'Меблі-Плюс'").Id, Price = 2480.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Стіл робочий (L-подібний)").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'Меблі-Плюс'").Id, Price = 4750.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Шафа для документів").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'Меблі-Плюс'").Id, Price = 3050.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Стілець офісний").Id, SupplierId = suppliers.Single(s => s.Name == "ФОП 'Все-до-офісу'").Id, Price = 2500.00m },

                new SupplierOffer { ProductId = products.Single(p => p.Name == "Сканер Canon").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'ТехноСвіт'").Id, Price = 2750.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Монітор 27'' Dell").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'ІТ-Компоненти'").Id, Price = 7150.00m },

                new SupplierOffer { ProductId = products.Single(p => p.Name == "Миша Logitech MX").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'ІТ-Компоненти'").Id, Price = 2050.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Клавіатура K380").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'ІТ-Компоненти'").Id, Price = 1480.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "USB-Хаб").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'ІТ-Компоненти'").Id, Price = 440.00m },

                new SupplierOffer { ProductId = products.Single(p => p.Name == "Миша Logitech MX").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'ТехноСвіт'").Id, Price = 2100.00m },

                new SupplierOffer { ProductId = products.Single(p => p.Name == "Степлер (великий)").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'КанцПром'").Id, Price = 310.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Набір ручок (10 шт, сині)").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'КанцПром'").Id, Price = 75.00m },

                new SupplierOffer { ProductId = products.Single(p => p.Name == "Мило рідке (5л)").Id, SupplierId = suppliers.Single(s => s.Name == "ФОП 'Все-до-офісу'").Id, Price = 245.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Рушники паперові (10 уп.)").Id, SupplierId = suppliers.Single(s => s.Name == "ФОП 'Все-до-офісу'").Id, Price = 295.00m },

                new SupplierOffer { ProductId = products.Single(p => p.Name == "Кавомашина Philips").Id, SupplierId = suppliers.Single(s => s.Name == "ТОВ 'ТехноСвіт'").Id, Price = 11400.00m },
                new SupplierOffer { ProductId = products.Single(p => p.Name == "Чайник електричний").Id, SupplierId = suppliers.Single(s => s.Name == "ФОП 'Все-до-офісу'").Id, Price = 780.00m },
            };
            supplierOffers.ForEach(so => context.SupplierOffers.Add(so));
            context.SaveChanges();

            // 6. Замовлення (з різними датами)
            var orders = new List<Order>
            {
                // Замовлення 1 (Старе, Оплачене)
                new Order { OrderDate = DateTime.Now.AddDays(-45), Description = "Планове замовлення канцтоварів", Status = OrderStatus.ДОСТАВЛЕНО, TotalAmount = 435.00m, UserId = users.Single(u => u.Login == "employee").Id },
                // Замовлення 2 (Старе, Оплачене, 2 позиції)
                new Order { OrderDate = DateTime.Now.AddDays(-30), Description = "Меблі для нового співробітника", Status = OrderStatus.ДОСТАВЛЕНО, TotalAmount = 7230.00m, UserId = users.Single(u => u.Login == "employee").Id },
                // Замовлення 3 (Старе, Оплачене)
                new Order { OrderDate = DateTime.Now.AddDays(-10), Description = "Замовлення для бухгалтерії (папір)", Status = OrderStatus.ДОСТАВЛЕНО, TotalAmount = 145.00m, UserId = users.Single(u => u.Login == "employee").Id },
                // Замовлення 4 (Скасоване - без рахунку)
                new Order { OrderDate = DateTime.Now.AddDays(-8), Description = "Терміново потрібна миша", Status = OrderStatus.СКАСОВАНО, TotalAmount = 2050.00m, UserId = users.Single(u => u.Login == "employee").Id },
                // Замовлення 5 (Недавнє, Очікує)
                new Order { OrderDate = DateTime.Now.AddDays(-5), Description = "Замовлення для відділу кадрів (принтер)", Status = OrderStatus.ВІДПРАВЛЕНО, TotalAmount = 4450.00m, UserId = users.Single(u => u.Login == "employee").Id },
                // Замовлення 6 (Недавнє, Очікує)
                new Order { OrderDate = DateTime.Now.AddDays(-2), Description = "Товари для кухні", Status = OrderStatus.ВІДПРАВЛЕНО, TotalAmount = 780.00m, UserId = users.Single(u => u.Login == "employee").Id },
                // Замовлення 7 (Нове, Очікує, 3 позиції)
                new Order { OrderDate = DateTime.Now.AddDays(-1), Description = "Нове робоче місце для розробника", Status = OrderStatus.ВІДПРАВЛЕНО, TotalAmount = 9070.00m, UserId = users.Single(u => u.Login == "employee").Id }
            };
            orders.ForEach(o => context.Orders.Add(o));
            context.SaveChanges();

            // 7. Позиції Замовлень
            var orderItems = new List<OrderItem>
            {
                // До Замовлення 1
                new OrderItem { Quantity = 3, UnitPrice = 145.00m, Amount = 435.00m, OrderId = orders[0].Id, SupplierOfferId = supplierOffers.Single(so => so.Price == 145.00m).Id },
                // До Замовлення 2
                new OrderItem { Quantity = 1, UnitPrice = 2480.00m, Amount = 2480.00m, OrderId = orders[1].Id, SupplierOfferId = supplierOffers.Single(so => so.Price == 2480.00m).Id },
                new OrderItem { Quantity = 1, UnitPrice = 4750.00m, Amount = 4750.00m, OrderId = orders[1].Id, SupplierOfferId = supplierOffers.Single(so => so.Price == 4750.00m).Id },
                // До Замовлення 3
                new OrderItem { Quantity = 1, UnitPrice = 145.00m, Amount = 145.00m, OrderId = orders[2].Id, SupplierOfferId = supplierOffers.Single(so => so.Price == 145.00m).Id },
                // До Замовлення 4
                new OrderItem { Quantity = 1, UnitPrice = 2050.00m, Amount = 2050.00m, OrderId = orders[3].Id, SupplierOfferId = supplierOffers.Single(so => so.Price == 2050.00m).Id },
                // До Замовлення 5
                new OrderItem { Quantity = 1, UnitPrice = 4450.00m, Amount = 4450.00m, OrderId = orders[4].Id, SupplierOfferId = supplierOffers.Single(so => so.Price == 4450.00m).Id },
                // До Замовлення 6
                new OrderItem { Quantity = 1, UnitPrice = 780.00m, Amount = 780.00m, OrderId = orders[5].Id, SupplierOfferId = supplierOffers.Single(so => so.Price == 780.00m).Id },
                // До Замовлення 7
                new OrderItem { Quantity = 1, UnitPrice = 7150.00m, Amount = 7150.00m, OrderId = orders[6].Id, SupplierOfferId = supplierOffers.Single(so => so.Price == 7150.00m).Id },
                new OrderItem { Quantity = 1, UnitPrice = 1480.00m, Amount = 1480.00m, OrderId = orders[6].Id, SupplierOfferId = supplierOffers.Single(so => so.Price == 1480.00m).Id },
                new OrderItem { Quantity = 1, UnitPrice = 440.00m, Amount = 440.00m, OrderId = orders[6].Id, SupplierOfferId = supplierOffers.Single(so => so.Price == 440.00m).Id }
            };
            orderItems.ForEach(oi => context.OrderItems.Add(oi));
            context.SaveChanges();

            // 8. Рахунки
            var invoices = new List<Invoice>
            {
                // До Замовлення 1
                new Invoice { OrderId = orders[0].Id, Amount = orders[0].TotalAmount, DueDate = orders[0].OrderDate.AddDays(14), PaymentStatus = PaymentStatus.ОПЛАЧЕНО },
                // До Замовлення 2
                new Invoice { OrderId = orders[1].Id, Amount = orders[1].TotalAmount, DueDate = orders[1].OrderDate.AddDays(14), PaymentStatus = PaymentStatus.ОПЛАЧЕНО },
                // До Замовлення 3
                new Invoice { OrderId = orders[2].Id, Amount = orders[2].TotalAmount, DueDate = orders[2].OrderDate.AddDays(14), PaymentStatus = PaymentStatus.ОПЛАЧЕНО },
                // Замовлення 4 - скасоване, рахунку немає
                // До Замовлення 5
                new Invoice { OrderId = orders[4].Id, Amount = orders[4].TotalAmount, DueDate = orders[4].OrderDate.AddDays(14), PaymentStatus = PaymentStatus.ОЧІКУЄТЬСЯ },
                // До Замовлення 6
                new Invoice { OrderId = orders[5].Id, Amount = orders[5].TotalAmount, DueDate = orders[5].OrderDate.AddDays(14), PaymentStatus = PaymentStatus.ОЧІКУЄТЬСЯ },
                // До Замовлення 7
                new Invoice { OrderId = orders[6].Id, Amount = orders[6].TotalAmount, DueDate = orders[6].OrderDate.AddDays(14), PaymentStatus = PaymentStatus.ОЧІКУЄТЬСЯ }
            };
            invoices.ForEach(i => context.Invoices.Add(i));
            context.SaveChanges();
        }
    }
}