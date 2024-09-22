using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class CategoryModel
    {
        [Key]
        public long id { get; set; }

        [Required, MaxLength(255)]
        public string name { get; set; }

        public string description { get; set; }

        [MaxLength(10)]
        public string status { get; set; } = "ACTIVE";
        public DateTime? createdDate { get; set; } = DateTime.Now;

        public DateTime? updatedDate { get; set; }

        public string createdBy { get; set; }

        public string updatedBy { get; set; }

        public ICollection<DishModel> dish { get; set; }
    }
}
