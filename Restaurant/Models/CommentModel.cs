using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("Comments")]
    public class CommentModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CommentId { get; set; } // Updated to CommentId

        [Required]
        [MaxLength(255)]
        public string UserName { get; set; }

        [Required]
        public string Content { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("Blog")] // Updated to use the navigation property directly
        public long BlogId { get; set; }
        public BlogModel? Blog { get; set; } // Ensure this property is included for navigation

        public long? ParentCommentId { get; set; }
        [ForeignKey(nameof(ParentCommentId))]
        public CommentModel? ParentComment { get; set; }
        public List<CommentModel> Replies { get; set; } = new List<CommentModel>();
    }
}
