using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    // Bảng nối có thêm dữ liệu riêng (Quantity, giá snapshot) nên PHẢI có class riêng,
    // khác với VenueFacilities (bảng nối thuần, không cần class).
    public class BookingAddOn
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("AddOnService")]
        public int AddOnId { get; set; }
        public virtual AddOnService AddOnService { get; set; }

        [Required]
        public int Quantity { get; set; }

        // Snapshot giá add-on tại thời điểm đặt — không đổi dù AddOnService.Price đổi sau này
        [Required]
        public decimal PriceAtBooking { get; set; }

        [Required]
        public decimal Subtotal { get; set; }
    }
}