using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("Blogs")] 
    public class BlogModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public long id { get; set; }

        [Required]
        [MaxLength(255)]
        public string title { get; set; }

        [Required]
        public string content { get; set; }

        [MaxLength(10)]
        public string status { get; set; } = "ACTIVE";

        [Column(TypeName = "datetime")] 
        public DateTime? createdDate { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")] 
        public DateTime? updatedDate { get; set; }

        [MaxLength(50)] 
        public string? createdBy { get; set; }

        [MaxLength(50)] 
        public string? updatedBy { get; set; }

        public string? banner { get; set; }
        [NotMapped] 
        public IFormFile? BannerUpload { get; set; }
        public int ViewCount { get; set; } = 0;
    }
}
