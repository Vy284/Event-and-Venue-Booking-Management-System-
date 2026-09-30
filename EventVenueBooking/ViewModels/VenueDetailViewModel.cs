using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace EventVenueBooking.ViewModels
{
    public class VenueDetailViewModel
    {
        public int VenueId { get; set; }
        public string Name { get; set; }
        public string VenueTypeName { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; }
        public decimal RentalRate { get; set; }
        public byte RentalUnit { get; set; }
        public string Location { get; set; }

        public List<string> ImageUrls { get; set; }
        public List<string> FacilityNames { get; set; }
        public List<AddOnServiceViewModel> AvailableAddOns { get; set; }
        public List<EventTypeViewModel> EventTypes { get; set; }
    }

    public class AddOnServiceViewModel
    {
        public int AddOnId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
    }

    public class EventTypeViewModel
    {
        public int EventTypeId { get; set; }
        public string TypeName { get; set; }
    }
}