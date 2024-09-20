using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class OrderModel
    {
        [Key]
        public long id { get; set; }

        public string message { get; set; }

        [MaxLength(10)]
        public string status { get; set; } = "Pending";
        public DateTime createdDate { get; set; } = DateTime.Now;

        public DateTime? updatedDate { get; set; }

        public string createdBy { get; set; }

        public string updatedBy { get; set; }

        public float? total { get; set; }

        [ForeignKey("user")]
        public long? userId { get; set; }
        public UserModel user { get; set; }

        public ICollection<OrderDetailModel> order_detail { get; set; }
    }
}
