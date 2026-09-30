using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class VenueFacility
    {
        public int VenueId { get; set; }
        public int FacilityId { get; set; }

        public virtual Venue Venue { get; set; }
        public virtual Facility Facility { get; set; }
    }
}