using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class VenueFacility
    {
        public int VenueId { get; set; }
        public int FacilityId { get; set; }

        // Foreign Keys & Navigation Properties
        [ForeignKey("VenueId")]
        public virtual Venue Venue { get; set; }

        [ForeignKey("FacilityId")]
        public virtual Facility Facility { get; set; }
    }
}