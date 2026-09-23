using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventVenueBooking.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Dashboard()
        {
            return View("~/Views/Admin/Dashboard.cshtml");
        }

        public ActionResult Bookings()
        {
            ViewBag.Title = "Bookings Management";
            ViewBag.ActiveMenu = "Bookings";

            return View("~/Views/Admin/Bookings.cshtml");
        }
    }
}