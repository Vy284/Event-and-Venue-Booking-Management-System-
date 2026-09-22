using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventVenueBooking.ViewModels
{
    public class VenueCardViewModel
    {
        public int VenueId { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; } // Lấy từ VenueType.TypeName
        public int Capacity { get; set; }
        public decimal RentalRate { get; set; }
        public byte RentalUnit { get; set; } // 0 = Hour, 1 = Session, 2 = Day
        public string Location { get; set; }
        public string ImageUrl { get; set; } // Ảnh từ VenueImages
    }
}