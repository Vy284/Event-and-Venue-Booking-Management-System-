using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventVenueBooking.ViewModels
{
    // Class đại diện cho mỗi dòng booking gần đây ở Dashboard
    public class RecentBookingViewModel
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; }
        public string VenueName { get; set; }
        public DateTime EventStartDateTime { get; set; }
        public decimal TotalCost { get; set; }
        public string StatusText { get; set; }
    }

    // Dùng cho màn hình Dashboard
    public class DashboardViewModel
    {
        public int TotalBookingsThisMonth { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveVenuesCount { get; set; }
        public List<RecentBookingViewModel> RecentBookings { get; set; } = new List<RecentBookingViewModel>();
    }

    // Dùng cho màn hình Calendar
    public class CalendarEventViewModel
    {
        public int BookingId { get; set; }
        public string VenueName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EventStatus { get; set; } // Ví dụ: "Confirmed", "Pending"
    }
}