namespace Restaurant.ViewModels
{
    public class CartItemViewModel
    {
        public long DishId { get; set; }
        public string Title { get; set; }
        public decimal? Price { get; set; }
        public int Quantity { get; set; }
        public string Banner { get; set; } 
    }
}