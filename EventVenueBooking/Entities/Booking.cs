using EventVenueBooking.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        // ---- Ai đặt ----
        [Required]
        [ForeignKey("ClientUser")]
        public int ClientUserId { get; set; }
        [InverseProperty("Bookings")]
        public virtual User ClientUser { get; set; }

        [Required]
        [ForeignKey("Venue")]
        public int VenueId { get; set; }
        public virtual Venue Venue { get; set; }

        [Required]
        [ForeignKey("EventType")]
        public int EventTypeId { get; set; }
        public virtual EventType EventType { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int GuestCount { get; set; }

        [Required]
        public DateTime EventStartDateTime { get; set; }

        [Required]
        public DateTime EventEndDateTime { get; set; }

        // 0=Pending, 1=Confirmed, 2=InProgress, 3=Completed, 4=Cancelled
        [Required]
        public byte Status { get; set; }

        // ---- Snapshot giá tại thời điểm đặt ----
        [Required]
        public decimal VenueRateAtBooking { get; set; }

        [Required]
        public byte RentalUnitAtBooking { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int RentalQuantity { get; set; }

        [Required]
        public decimal VenueCost { get; set; }

        [Required]
        public decimal AddOnCost { get; set; } = 0;

        [Required]
        public decimal TotalCost { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ---- Audit: ai sửa / ai hủy ----
        [ForeignKey("LastModifiedByUser")]
        public int? LastModifiedByUserId { get; set; }
        [InverseProperty("ModifiedBookings")]
        public virtual User LastModifiedByUser { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        [ForeignKey("CancelledByUser")]
        public int? CancelledByUserId { get; set; }
        [InverseProperty("CancelledBookings")]
        public virtual User CancelledByUser { get; set; }

        public DateTime? CancelledAt { get; set; }

        [MaxLength(500)]
        public string CancelReason { get; set; }

        // ---- Navigation properties ----
        public virtual ICollection<BookingAddOn> BookingAddOns { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        //public virtual Feedback Feedback { get; set; } // 1 - 0..1

        public Booking()
        {
            BookingAddOns = new HashSet<BookingAddOn>();
            Payments = new HashSet<Payment>();
        }
    }
}