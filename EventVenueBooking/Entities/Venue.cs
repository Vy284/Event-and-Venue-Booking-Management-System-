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

        [Required(ErrorMessage = "Vui lòng nhập tên địa điểm.")]
        [MaxLength(150, ErrorMessage = "Tên địa điểm không được vượt quá 150 ký tự.")]
        public string Name { get; set; }

        [Required]
        [ForeignKey("VenueType")]
        public int VenueTypeId { get; set; }
        public virtual VenueType VenueType { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập sức chứa.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải lớn hơn 0.")]
        public int Capacity { get; set; }

        public string Description { get; set; }

        // EF6 default convention maps decimal as decimal(18,2) — đủ dùng, không cần khai báo thêm
        [Required(ErrorMessage = "Vui lòng nhập giá thuê.")]
        [Range(typeof(decimal), "0", "79228162514264337593543950335",
            ErrorMessage = "Giá thuê không được nhỏ hơn 0.")]
        public decimal RentalRate { get; set; }

        // 0 = Hour, 1 = Session, 2 = Day
        [Required(ErrorMessage = "Vui lòng chọn đơn vị tính.")]
        [Range(0, 2, ErrorMessage = "Đơn vị tính không hợp lệ.")]
        public byte RentalUnit { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập vị trí.")]
        [MaxLength(255, ErrorMessage = "Vị trí không được vượt quá 255 ký tự.")]
        public string Location { get; set; }

        // 0 = Inactive, 1 = Active, 2 = UnderMaintenance
        [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
        [Range(0, 2, ErrorMessage = "Trạng thái không hợp lệ.")]
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