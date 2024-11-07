using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("Message")]
    public class MessageModel
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [Required]
        public long SenderId { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsSeen { get; set; } = false;

        public bool IsAdmin { get; set; } = false;

        [ForeignKey("ConversationId")]
        public ConversationModel? Conversation { get; set; }

        [ForeignKey("SenderId")]
        public UserModel? Sender { get; set; }
    }
}
