using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class OrderDetailViewModel
{
    [Required(ErrorMessage = "Dish Name is required.")]
    public string DishName { get; set; }

    [Required(ErrorMessage = "Dish ID is required.")]
    public long DishId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    public decimal? Price { get; set; }

    [Display(Name = "Total Price")]
    public decimal? Total { get; set; }
}

}