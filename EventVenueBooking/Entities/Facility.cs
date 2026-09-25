using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class Facility
    {
        [Key]
        public int FacilityId { get; set; }

        [Required]
        [MaxLength(100)]
        [Index(IsUnique = true)]
        public string FacilityName { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<VenueFacility> VenueFacilities { get; set; }

        public Facility()
        {
            VenueFacilities = new HashSet<VenueFacility>();
        }
    }
}