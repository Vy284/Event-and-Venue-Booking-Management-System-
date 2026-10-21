using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        // UNIQUE — mỗi Booking chỉ được 1 Feedback
        [Required(ErrorMessage = "Không tìm thấy booking cần đánh giá.")]
        [Index(IsUnique = true)]
        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá.")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao.")]
        public byte Rating { get; set; } // 1-5, validate ở code (DAL/Service)

        public string Comment { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}