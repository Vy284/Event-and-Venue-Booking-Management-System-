using EventVenueBooking.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required]
        [ForeignKey("VenueType")]
        public int VenueTypeId { get; set; }
        public virtual VenueType VenueType { get; set; }

        [Required]
        public int Capacity { get; set; }

        public string Description { get; set; }

        // EF6 default convention maps decimal as decimal(18,2) — đủ dùng, không cần khai báo thêm
        [Required]
        public decimal RentalRate { get; set; }

        // 0 = Hour, 1 = Session, 2 = Day
        [Required]
        public byte RentalUnit { get; set; }

        [Required]
        [MaxLength(255)]
        public string Location { get; set; }

        // 0 = Active, 1 = Inactive, 2 = UnderMaintenance
        [Required]
        public byte Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [ForeignKey("CreatedByUser")]
        public int CreatedByUserId { get; set; }
        [InverseProperty("CreatedVenues")]
        public virtual User CreatedByUser { get; set; }

        // ---- Navigation properties ----
        public virtual ICollection<VenueImage> VenueImages { get; set; }

        public virtual ICollection<VenueFacility> VenueFacilities { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }

        public Venue()
        {
            VenueImages = new HashSet<VenueImage>();
            VenueFacilities = new HashSet<VenueFacility>();
            Bookings = new HashSet<Booking>();
        }
    }
}