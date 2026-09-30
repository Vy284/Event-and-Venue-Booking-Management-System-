using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace EventVenueBooking.ViewModels
{
    public class FeedbackViewModel
    {
        public int FeedbackId { get; set; }
        public int BookingId { get; set; }
        public string CustomerName { get; set; }
        public string VenueName { get; set; }

        [Range(1, 5, ErrorMessage = "Vui lòng chọn từ 1 đến 5 sao")]
        public byte Rating { get; set; }
        public string Comment { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}