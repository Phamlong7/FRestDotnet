using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class UserModel
    {
        [Key]
        public long id { get; set; }

        [Required, MaxLength(255)]
        public string username { get; set; }

        [Required, MaxLength(255)]
        public string password { get; set; }

        [MaxLength(10)]
        public string role { get; set; } = "USER";

        [Required, MaxLength(255)]
        public string email { get; set; }
        public DateTime? createdDate { get; set; } = DateTime.Now;

        public DateTime? updatedDate { get; set; }

        public string createdBy { get; set; }

        public string updatedBy { get; set; }

        [MaxLength(10)]
        public string status { get; set; } = "ACTIVE";

        public ICollection<OrderModel> order { get; set; }
    }
}
