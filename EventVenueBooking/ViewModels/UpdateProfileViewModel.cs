using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EventVenueBooking.ViewModels
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Họ và tên.")]
        [MaxLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; }

        [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        public string Phone { get; set; }
    }
}