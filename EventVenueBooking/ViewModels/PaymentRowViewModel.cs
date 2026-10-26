using System;

namespace EventVenueBooking.ViewModels
{
    public class PaymentRowViewModel
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public byte PaymentType { get; set; }
        public string PaymentMethod { get; set; }
        public byte PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string RecordedBy { get; set; }
    }
}