using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class VenueImage
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        [ForeignKey("Venue")]
        public int VenueId { get; set; }
        public virtual Venue Venue { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; }

        // Ràng buộc "chỉ 1 IsPrimary=true / venue" xử lý ở code (DAL), không có ở DB
        [Required]
        public bool IsPrimary { get; set; } = false;
    }
}