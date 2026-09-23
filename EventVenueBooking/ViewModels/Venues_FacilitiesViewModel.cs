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

        // Lấy từ VenueType.cs
        public string TypeName { get; set; }

        // Lấy từ VenueImage.cs (chỉ lấy IsPrimary = true)
        public string PrimaryImageUrl { get; set; }

        public bool IsActive { get; set; }

        // Bao gồm luôn Facilities & Services
        public List<string> Facilities { get; set; } = new List<string>();
    }
}