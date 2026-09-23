using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventVenueBooking.ViewModels
{
    // Quản lý thông tin đặt chỗ và trạng thái thanh toán
    public class BookingManagementViewModel
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; }
        public string VenueName { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalAmount { get; set; }

        // Gom luôn trạng thái payment vào đây
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; } // "Paid", "Unpaid", "Refunded"
    }
}