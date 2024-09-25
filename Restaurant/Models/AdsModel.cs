using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("Ads")] // Tên bảng trong cơ sở dữ liệu
    public class AdsModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tự động tạo giá trị cho khóa chính
        public long id { get; set; }

        [Required] // Bắt buộc
        public string images { get; set; }

        [MaxLength(10)]
        public string status { get; set; } = "ACTIVE";

        [Column(TypeName = "datetime")] // Kiểu dữ liệu
        public DateTime? createdDate { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")] // Kiểu dữ liệu
        public DateTime? updatedDate { get; set; }

        [MaxLength(50)] // Giới hạn độ dài cho tên người tạo
        public string createdBy { get; set; }

        [MaxLength(50)] // Giới hạn độ dài cho tên người cập nhật
        public string updatedBy { get; set; }

        public int? width { get; set; }
        public int? height { get; set; }

        [MaxLength(20)] // Giới hạn độ dài cho vị trí
        public string position { get; set; }

        [Url] // Kiểm tra định dạng URL hợp lệ
        public string url { get; set; }
    }
}
