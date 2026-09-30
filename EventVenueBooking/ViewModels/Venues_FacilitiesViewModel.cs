using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventVenueBooking.ViewModels
{
    // Hiển thị danh sách Venues kèm ảnh chính và loại
    public class VenueListViewModel
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerHour { get; set; }

        // Thêm trường hiển thị Unit (Giờ/Buổi/Ngày)
        public int RentalUnit { get; set; }
        public string TypeName { get; set; }
        public string PrimaryImageUrl { get; set; }

        // Sửa lại thành Status kiểu int để biết Active (0), Inactive (1), Maintenance (2)
        public int Status { get; set; }

        // Thêm các trường cho UI
        public string Location { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; } // Sẽ tính trung bình từ Feedback
        public DateTime? LastBooking { get; set; } // Ngày đặt gần nhất
        public List<string> Facilities { get; set; } = new List<string>();
    }

    public class FacilitiesServicesViewModel
    {
        public List<FacilityViewModel> Facilities { get; set; }
        public List<AddOnServiceViewModel> Services { get; set; }
    }

    public class FacilityViewModel
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public bool IsActive { get; set; } // Trong entity[cite: 53] kiểu bool

        // Thêm trường này để hiển thị có bao nhiêu Venue đang dùng Facility này
        public int LinkedVenuesCount { get; set; }
    }

    public class AddOnServiceViewModel
    {
        public int AddOnId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; } // Trong entity[cite: 52] kiểu bool
    }
}