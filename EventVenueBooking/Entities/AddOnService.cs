using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventVenueBooking.Entities
{
    public class AddOnService
    {
        [Key]
        public int AddOnId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ.")]
        [MaxLength(150, ErrorMessage = "Tên dịch vụ không được vượt quá 150 ký tự.")]
        [Index(IsUnique = true)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá dịch vụ.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá dịch vụ không được nhỏ hơn 0.")]
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