using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcurementSystem.Models.Enums; // Підключаємо наші enum-и

namespace ProcurementSystem.Models
{ 
    public class User
    {
        public int Id { get; set; } 
        public string Login { get; set; }
        public string Password { get; set; } 
        public UserRole Role { get; set; } 

        public virtual ICollection<Order> Orders { get; set; }
    }
}