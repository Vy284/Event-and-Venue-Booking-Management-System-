using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EventVenueBooking.ViewModels
{
    public class ChangeEmailViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email mới.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [MaxLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
        public string NewEmail { get; set; }
    }
}