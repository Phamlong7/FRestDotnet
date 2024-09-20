using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class WebSettingModel
    {
        [Key]
        public long id { get; set; }

        [Required]
        public string content { get; set; }
        public DateTime createdDate { get; set; } = DateTime.Now;

        public DateTime? updatedDate { get; set; }

        public string createdBy { get; set; }

        public string updatedBy { get; set; }

        [MaxLength(10)]
        public string status { get; set; } = "ACTIVE";

        public string type { get; set; }
        public string image { get; set; }
    }
}
