using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace EventVenueBooking.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        [Compare("PasswordHash", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}