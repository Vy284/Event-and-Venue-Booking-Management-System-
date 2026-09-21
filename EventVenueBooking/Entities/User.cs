using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(255)]
        [Index(IsUnique = true)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        // 0 = Client, 1 = Coordinator, 2 = Admin
        [Required]
        public byte Role { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ---- Navigation properties ----
        // Users -> Venues (CreatedByUserId)
        [InverseProperty("CreatedByUser")]
        public virtual ICollection<Venue> CreatedVenues { get; set; }

        // Users -> Bookings (ClientUserId)
        [InverseProperty("ClientUser")]
        public virtual ICollection<Booking> Bookings { get; set; }

        // Users -> Bookings (LastModifiedByUserId)
        [InverseProperty("LastModifiedByUser")]
        public virtual ICollection<Booking> ModifiedBookings { get; set; }

        // Users -> Bookings (CancelledByUserId)
        [InverseProperty("CancelledByUser")]
        public virtual ICollection<Booking> CancelledBookings { get; set; }

        // Users -> Payments (RecordedByUserId)
        [InverseProperty("RecordedByUser")]
        public virtual ICollection<Payment> RecordedPayments { get; set; }

        public User()
        {
            CreatedVenues = new HashSet<Venue>();
            Bookings = new HashSet<Booking>();
            ModifiedBookings = new HashSet<Booking>();
            CancelledBookings = new HashSet<Booking>();
            RecordedPayments = new HashSet<Payment>();
        }
    }
}


