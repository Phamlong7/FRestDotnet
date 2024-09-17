using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required,MinLength (4,ErrorMessage ="Please Enter Name !")] 
        public string Name { get; set; } 
        [Required,MinLength(4, ErrorMessage = "Please Enter Description !")]
        public string Description { get; set; }
        public string Status { get; set; }
    }
}
