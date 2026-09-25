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

        // N-N với Venue, EF6 tự tạo bảng nối VenueFacilities (cấu hình ở DbContext)
        public virtual ICollection<Venue> Venues { get; set; }

        public Facility()
        {
            Venues = new HashSet<Venue>();
        }
    }
}