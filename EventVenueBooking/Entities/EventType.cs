using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class EventType
    {
        [Key]
        public int EventTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        [Index(IsUnique = true)]
        public string TypeName { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Booking> Bookings { get; set; }

        public EventType()
        {
            Bookings = new HashSet<Booking>();
        }
    }
}