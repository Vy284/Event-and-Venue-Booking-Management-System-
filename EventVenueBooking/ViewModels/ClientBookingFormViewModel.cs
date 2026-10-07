using System;
using System.Collections.Generic;
using EventVenueBooking.Entities;
using EventVenueBooking.Services;

namespace EventVenueBooking.ViewModels
{
    // Dữ liệu cho form đặt sảnh của Client
    public class ClientBookingFormViewModel
    {
        // Các trường client gửi lên
        public int VenueId { get; set; }
        public int EventTypeId { get; set; }
        public int GuestCount { get; set; }
        public DateTime? EventStart { get; set; }
        public DateTime? EventEnd { get; set; }
        public List<AddOnRequest> AddOns { get; set; } = new List<AddOnRequest>();

        // Các trường server nạp để hiển thị (không nhận từ client)
        public string VenueName { get; set; }
        public decimal RentalRate { get; set; }
        public byte RentalUnit { get; set; }
        public int Capacity { get; set; }
        public List<EventType> EventTypes { get; set; }
        public List<AddOnService> AvailableAddOns { get; set; }
    }
}