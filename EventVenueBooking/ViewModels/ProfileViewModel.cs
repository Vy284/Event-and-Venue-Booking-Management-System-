using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventVenueBooking.ViewModels
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public byte Role { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<UserBookingViewModel> Bookings { get; set; } = new List<UserBookingViewModel>();
    }

    public class UserBookingViewModel
    {
        public int BookingId { get; set; }
        public string VenueName { get; set; }
        public string VenueImageUrl { get; set; }
        public string EventTypeName { get; set; }
        public DateTime EventStartDateTime { get; set; }
        public DateTime EventEndDateTime { get; set; }
        public int GuestCount { get; set; }
        public decimal TotalCost { get; set; }
        public byte Status { get; set; }
    }
}