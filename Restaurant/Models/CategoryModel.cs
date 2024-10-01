using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("Category")] // Tên bảng trong cơ sở dữ liệu
    public class CategoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tự động tạo giá trị cho khóa chính
        public long id { get; set; }

        [Required]
        [MaxLength(255)]
        public string name { get; set; }

        public string description { get; set; }

        [MaxLength(10)]
        public string status { get; set; } = "ACTIVE";

        [Column(TypeName = "datetime")] // Kiểu dữ liệu
        public DateTime? createdDate { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")] // Kiểu dữ liệu
        public DateTime? updatedDate { get; set; }

        [MaxLength(50)]
        [ValidateNever]// Giới hạn độ dài cho tên người tạo
        public string createdBy { get; set; }

        [MaxLength(50)] // Giới hạn
        [ValidateNever]// Giới hạn độ dài cho tên người tạo
                       // độ dài cho tên người cập nhật
        public string? updatedBy { get; set; }

        // Quan hệ một-nhiều với DishModel
        [ValidateNever]
        public ICollection<DishModel> dish { get; set; }
    }
}
