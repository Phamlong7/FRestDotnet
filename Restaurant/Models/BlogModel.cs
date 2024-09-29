using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("Blogs")] // Tên bảng trong cơ sở dữ liệu
    public class BlogModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tự động tạo giá trị cho khóa chính
        public long id { get; set; }

        [Required]
        [MaxLength(255)]
        public string title { get; set; }

        [Required]
        public string content { get; set; }

        [MaxLength(10)]
        public string status { get; set; } = "ACTIVE";

        [Column(TypeName = "datetime")] // Kiểu dữ liệu
        public DateTime? createdDate { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")] // Kiểu dữ liệu
        public DateTime? updatedDate { get; set; }

        [MaxLength(50)] // Giới hạn độ dài cho tên người tạo
        public string? createdBy { get; set; }

        [MaxLength(50)] // Giới hạn độ dài cho tên người cập nhật
        public string? updatedBy { get; set; }

        public string? banner { get; set; }
        [NotMapped] // not map to database
        public IFormFile? BannerUpload { get; set; }
    }
}
