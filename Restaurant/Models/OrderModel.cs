using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("Order")]
    public class OrderModel
    {
        [Key]
        public long id { get; set; } // Use PascalCase for property names

        public string message { get; set; } // Use PascalCase for property names

        [MaxLength(10)]
        public string status { get; set; } = "Pending"; // Use PascalCase for property names

        public DateTime? createdDate { get; set; } = DateTime.Now; // Use PascalCase for property names

        public DateTime? updatedDate { get; set; } // Nullable for optional

        public string createdBy { get; set; } // Use PascalCase for property names

        public string updatedBy { get; set; } // Use PascalCase for property names

        public decimal? total { get; set; } // Nullable for optional total

        [ForeignKey("User")] // Foreign key to UserModel
        public long? userId { get; set; } // Use PascalCase for property names
        public UserModel user { get; set; } // Navigation property to UserModel

        public ICollection<OrderDetailModel> orderDetails { get; set; } // Collection of order details
    }
}
