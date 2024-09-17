using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class OrderModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Message { get; set; }
        public string Status { get; set; }
        [Required]
        public Double Total { get; set; }
        public int UserId { get; set; }
        public UserModel User { get; set; }
    }
}
