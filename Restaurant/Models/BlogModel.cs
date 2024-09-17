using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class BlogModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public string Status { get; set; }
        [Required]
        public string Banner {  get; set; }
        public int NumberAccess { get; set; }

    }
}
