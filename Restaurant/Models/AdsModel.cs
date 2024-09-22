using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class AdsModel
    {
        [Key]
        public long id { get; set; }

        public string images { get; set; }

        [MaxLength(10)]
        public string status { get; set; } = "ACTIVE";
        public DateTime? createdDate { get; set; } = DateTime.Now;

        public DateTime? updatedDate { get; set; }

        public string createdBy { get; set; }

        public string updatedBy { get; set; }

        public int? width { get; set; }
        public int? height { get; set; }
        public string position { get; set; }
        public string url { get; set; }
    }
}
