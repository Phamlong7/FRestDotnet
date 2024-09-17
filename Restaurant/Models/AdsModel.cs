using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class AdsModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Images { get; set; }
        [Required]
        public int Width { get; set; }
        [Required]
        public int Height { get; set; }
        [Required]
        public string Position { get; set; }
        [Required]
        public string Url { get; set; }
        public string Status { get; set; }

    }
}
