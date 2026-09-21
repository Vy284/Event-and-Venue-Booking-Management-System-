using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class VenueType
    {
        [Key]
        public int VenueTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        [Index(IsUnique = true)]
        public string TypeName { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Venue> Venues { get; set; }

        public VenueType()
        {
            Venues = new HashSet<Venue>();
        }
    }
}