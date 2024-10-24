using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class ExternalLoginRegistrationViewModel
    {
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
