using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("User")]
    public class UserModel : IdentityUser<long> 
    {
        [Key]
        public override long Id { get; set; } 

        [Required]
        [MaxLength(255)]
        public override string UserName { get; set; } 

        [MaxLength(10)]
        public string Role { get; set; } = "USER"; 

        public DateTime CreatedDate { get; set; } = DateTime.Now; 

        public DateTime? UpdatedDate { get; set; } 

        [MaxLength(50)]
        public string? CreatedBy { get; set; } = "USER";

        [MaxLength(50)]
        public string? UpdatedBy { get; set; }

        [MaxLength(10)]
        public string Status { get; set; } = "ACTIVE"; 

        public ICollection<OrderModel> Orders { get; set; }
        public ICollection<ConversationModel>? Conversations { get; set; }
    }
}
