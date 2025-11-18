using System.ComponentModel.DataAnnotations;

namespace ProcurementSystem.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "*Ім'я є обов'язковим")]
        [Display(Name = "Ім'я")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "*Прізвище є обов'язковим")]
        [Display(Name = "Прізвище")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email є необов'язковим")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email")]
        [Display(Name = "Email")]
        [StringLength(100)]
        public string Email { get; set; }
    }
}