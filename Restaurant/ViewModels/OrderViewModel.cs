
namespace Restaurant.ViewModels
{
    public class OrderViewModel
    {
        public long? OrderId { get; set; }
        public string? Message { get; set; }
        public List<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();
        public string? Status { get; set; } // Include this to hold the order status
        public DateTime? CreatedDate { get; set; }  // Added CreatedDate property
         public long? UserId { get; set; }          // Added UserId property for the view
        public decimal? Total { get; set; }           // Added Total property for the view
    }
}