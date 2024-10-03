namespace Restaurant.ViewModels
{
    public class OrderViewModel
    {
        public long? OrderId { get; set; }
        public string? Message { get; set; }
        public List<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();
        public string? Status { get; set; } 
        public DateTime? CreatedDate { get; set; }  
        public long? UserId { get; set; }         
        public decimal? Total { get; set; }           
    }
}