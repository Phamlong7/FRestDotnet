using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class OrderDetailModel
    {
        [Key]
        [Column(Order = 0)]
        public long orderId { get; set; }

        [Key]
        [Column(Order = 1)]
        public long dishId { get; set; }

        public int quantity { get; set; }

        [ForeignKey("orderId")]
        public OrderModel order { get; set; }

        [ForeignKey("dishId")]
        public DishModel dish { get; set; }
    }
}
