using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventVenueBooking.ViewModels
{
    public class BookingRequestViewModel
    {
        public int VenueId { get; set; }
        public int EventTypeId { get; set; }
        public int GuestCount { get; set; }
        public DateTime EventStartDateTime { get; set; }
        public DateTime EventEndDateTime { get; set; }
        public List<int> SelectedAddOnIds { get; set; }
    }
}