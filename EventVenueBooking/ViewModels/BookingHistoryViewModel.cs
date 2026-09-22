using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventVenueBooking.ViewModels
{
    public class BookingHistoryViewModel
    {
            public int BookingId { get; set; }
            public string VenueName { get; set; }
            public string EventTypeName { get; set; }
            public DateTime EventStartDateTime { get; set; }
            public DateTime EventEndDateTime { get; set; }
            public decimal TotalCost { get; set; }

            // Status: 0=Pending, 1=Confirmed, 2=InProgress, 3=Completed, 4=Cancelled
            public byte Status { get; set; }
            public DateTime CreatedAt { get; set; }
    }
}