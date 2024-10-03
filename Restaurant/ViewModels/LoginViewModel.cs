using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email or Username is required.")]
        public string EmailOrUsername { get; set; } // Allow either email or username

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
