using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("Conversation")]
    public class ConversationModel
    {
        [Key]
        public int ConversationId { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public long AdminId { get; set; }

        [Required]
        public DateTime LastMessageTime { get; set; }

        public string LastMessage { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey("UserId")]
        public UserModel? User { get; set; }

        [ForeignKey("AdminId")]
        public UserModel? Admin { get; set; }

        public ICollection<MessageModel> Messages { get; set; }
    }
}
