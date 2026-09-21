using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class AddOnService
    {
        [Key]
        public int AddOnId { get; set; }

        [Required]
        [MaxLength(150)]
        [Index(IsUnique = true)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<BookingAddOn> BookingAddOns { get; set; }

        public AddOnService()
        {
            BookingAddOns = new HashSet<BookingAddOn>();
        }
    }
}