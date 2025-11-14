using System.ComponentModel.DataAnnotations;

namespace ProcurementSystem.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Поле Логін є обов'язковим")]
        [Display(Name = "Логін")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Поле Пароль є обов'язковим")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
    }
}