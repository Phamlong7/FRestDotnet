using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("Dish")] // Tên bảng trong cơ sở dữ liệu
    public class DishModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tự động tạo giá trị cho khóa chính
        public long id { get; set; }  // Sửa từ 'object' thành 'long'

        [Required]
        [MaxLength(255)]
        public string title { get; set; }

        [Required]
        public string content { get; set; }

        [MaxLength(10)]
        public string status { get; set; } = "ACTIVE";

        [Column(TypeName = "datetime")] // Kiểu dữ liệu cho ngày tạo
        public DateTime? createdDate { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")] // Kiểu dữ liệu cho ngày cập nhật
        public DateTime? updatedDate { get; set; }

        [MaxLength(50)]
        [ValidateNever]
        public string? createdBy { get; set; }

        [MaxLength(50)]
        [ValidateNever]
        public string? updatedBy { get; set; }

        [ValidateNever]
        public string? banner { get; set; }

        [NotMapped]
        public IFormFile BannerUpload { get; set; } // Thuộc tính cho file upload

        [ForeignKey("Category")] // Chỉ định khóa ngoại
        public long? categoryId { get; set; }

        public decimal? price { get; set; }

        // Thuộc tính điều hướng tới CategoryModel
        [ValidateNever]
        public CategoryModel category { get; set; }

        // Quan hệ với OrderDetailModel
        [ValidateNever]
        public ICollection<OrderDetailModel> orderDetails { get; set; }
    }
}
