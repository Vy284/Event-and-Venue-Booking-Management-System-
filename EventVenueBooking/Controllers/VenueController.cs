using EventVenueBooking.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace EventVenueBooking.Controllers
{
    public class VenueController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Venue/Details/1
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var venue = db.Venues
                .Include(v => v.VenueType)
                .Include(v => v.Facilities)
                .FirstOrDefault(v => v.VenueId == id);

            if (venue == null)
            {
                return HttpNotFound();
            }

            ViewBag.Images = db.VenueImages.Where(img => img.VenueId == id).ToList();
            ViewBag.AddOnServices = db.AddOnServices.Where(a => a.IsActive).ToList();

            return View(venue);
        }

        // GET: /Venue/GetVenueBookings?venueId
        [HttpGet]
        public JsonResult GetVenueBookings(int venueId)
        {
            // Lấy tất cả các Booking của sảnh này ngoại trừ các đơn đã bị Hủy (Status = 4)
            var bookings = db.Bookings
                .Where(b => b.VenueId == venueId && b.Status != 4)
                .Select(b => new
                {
                    title = "Đã được đặt",
                    start = b.EventStartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = b.EventEndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = "#dc3545", // Màu đỏ cho bận
                    textColor = "#ffffff"
                })
                .ToList();

            return Json(bookings, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}