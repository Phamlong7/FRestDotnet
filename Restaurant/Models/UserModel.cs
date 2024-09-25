using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("User")]
    public class UserModel : IdentityUser<long> // Kế thừa từ IdentityUser<long> để sử dụng ID kiểu long
    {
        [Key]
        public override long Id { get; set; } // Sử dụng thuộc tính Id từ IdentityUser, với kiểu long

        [Required]
        [MaxLength(255)]
        public override string UserName { get; set; } // Đổi sang PascalCase, kế thừa từ IdentityUser

        [MaxLength(10)]
        public string Role { get; set; } = "USER"; // Thuộc tính role, mặc định là "USER"

        public DateTime CreatedDate { get; set; } = DateTime.Now; // Ngày tạo người dùng

        public DateTime? UpdatedDate { get; set; } // Ngày cập nhật, có thể để trống

        [MaxLength(50)]
        public string CreatedBy { get; set; } = "USER";

        [MaxLength(50)]
        public string UpdatedBy { get; set; } = "USER";

        [MaxLength(10)]
        public string Status { get; set; } = "ACTIVE"; // Trạng thái người dùng, mặc định là "ACTIVE"

        // Quan hệ một-nhiều với OrderModel
        public ICollection<OrderModel> Orders { get; set; }
    }
}
