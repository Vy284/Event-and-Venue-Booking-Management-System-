using EventVenueBooking.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        [Required]
        public decimal Amount { get; set; }

        // 0 = Deposit, 1 = FinalPayment
        [Required]
        public byte PaymentType { get; set; }

        [MaxLength(50)]
        public string PaymentMethod { get; set; }

        // 0 = Pending, 1 = Completed, 2 = Refunded
        [Required]
        public byte PaymentStatus { get; set; }

        // Nullable vì Pending chưa có thời điểm nhận tiền thật
        public DateTime? PaymentDate { get; set; }

        [Required]
        [ForeignKey("RecordedByUser")]
        public int RecordedByUserId { get; set; }
        [InverseProperty("RecordedPayments")]
        public virtual User RecordedByUser { get; set; }
    }
}