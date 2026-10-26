using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EventVenueBooking.ViewModels
{
    public class PaymentCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Booking ID.")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền.")]
        [Range(0.01, 999999999, ErrorMessage = "Số tiền phải lớn hơn 0.")]
        public decimal Amount { get; set; }

        [Range(0, 1, ErrorMessage = "Loại thanh toán không hợp lệ.")]
        public byte PaymentType { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [MaxLength(50)]
        public string PaymentMethod { get; set; }

        [Range(0, 1, ErrorMessage = "Khi tạo mới chỉ được chọn Pending hoặc Completed.")]
        public byte PaymentStatus { get; set; }
    }
}