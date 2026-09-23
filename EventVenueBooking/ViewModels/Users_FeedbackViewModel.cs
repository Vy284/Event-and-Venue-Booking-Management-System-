using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventVenueBooking.ViewModels
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }

    public class FeedbackViewModel
    {
        public int FeedbackId { get; set; }
        public string CustomerName { get; set; }
        public string VenueName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}