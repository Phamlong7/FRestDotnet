using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("WebSetting")]
    public class WebSettingModel
    {
        [Key]
        public long id { get; set; } // Sử dụng PascalCase cho thuộc tính Id

        [Required]
        public string content { get; set; } // Sử dụng PascalCase cho thuộc tính Content

        public DateTime? createdDate { get; set; } = DateTime.Now; // Sử dụng PascalCase cho thuộc tính CreatedDate

        public DateTime? updatedDate { get; set; } // Giữ nguyên nullable cho tùy chọn

        public string? createdBy { get; set; } // Sử dụng PascalCase cho thuộc tính CreatedBy

        public string? updatedBy { get; set; } // Sử dụng PascalCase cho thuộc tính UpdatedBy

        [MaxLength(10)]
        public string status { get; set; } = "ACTIVE"; // Sử dụng PascalCase cho thuộc tính Status

        public string type { get; set; } // Sử dụng PascalCase cho thuộc tính Type

        [Url]
        public string? image { get; set; } // Sử dụng PascalCase cho thuộc tính Image

        [NotMapped] // NotMapped since we don't store the actual file in the database
        public IFormFile? imageUpload { get; set; }
    }
}
