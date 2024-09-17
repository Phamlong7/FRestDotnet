using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class DishModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public string Status { get; set; }
        [Required]
        public string Banner { get; set; }
        [Required]
        public decimal Price { get; set; }
        public int Categoryid { get; set; }
        public CategoryModel Category { get; set; }
    }
}
