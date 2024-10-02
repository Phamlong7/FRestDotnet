using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("OrderDetails")] // Tên bảng trong cơ sở dữ liệu
    public class OrderDetailModel
    {
        [Key]
        [Column(Order = 0)] // Xác định thứ tự của khóa chính
        public long? orderId { get; set; }

        [Key]
        [Column(Order = 1)] // Xác định thứ tự của khóa chính
        public long dishId { get; set; }

        public int quantity { get; set; }

        public decimal? priceAtOrder {get; set; }
        
        
        // Quan hệ với OrderModel
        [ForeignKey("orderId")]
        public OrderModel order { get; set; }

        // Quan hệ với DishModel
        [ForeignKey("dishId")]
        public DishModel dish { get; set; }
    }
}
