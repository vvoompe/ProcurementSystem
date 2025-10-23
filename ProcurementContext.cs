using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ProcurementSystem.Models;
using ProcurementSystem.Models.Enums;

namespace ProcurementSystem.Data 
{

    public class ProcurementContext : DbContext
    {

        public ProcurementContext() : base("DbConnection")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierOffer> SupplierOffers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportOrder> ReportOrders { get; set; }
    }
}