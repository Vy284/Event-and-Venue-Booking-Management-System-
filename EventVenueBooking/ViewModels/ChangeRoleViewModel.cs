using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EventVenueBooking.ViewModels
{
    public class ChangeRoleViewModel
    {
        public int UserId { get; set; }

        [Required]
        public byte Role { get; set; }
    }
}