using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }
}
