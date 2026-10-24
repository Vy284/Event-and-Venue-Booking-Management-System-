using EventVenueBooking.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace EventVenueBooking.Controllers
{
    [AllowAnonymous] // THÊM: khách chưa đăng nhập vẫn xem được sảnh và lịch
    public class VenueController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Venue/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var venue = db.Venues
               .Include(v => v.VenueType)
               .Include(v => v.VenueFacilities.Select(vf => vf.Facility))
               .FirstOrDefault(v => v.VenueId == id);

            if (venue == null)
            {
                return HttpNotFound();
            }

            ViewBag.Images = db.VenueImages.Where(img => img.VenueId == id).ToList();
            ViewBag.AddOnServices = db.AddOnServices.Where(a => a.IsActive).ToList();

            // THÊM: đánh giá của khách cho sảnh này. Feedback gắn với Booking nên lọc theo Booking.VenueId.
            // Điểm trung bình và số đánh giá tính trên TẤT CẢ feedback; danh sách chỉ hiện 20 đánh giá mới nhất.
            var venueFeedbacks = db.Feedbacks.Where(f => f.Booking.VenueId == id.Value);
            int reviewCount = venueFeedbacks.Count();
            ViewBag.ReviewCount = reviewCount;
            ViewBag.AverageRating = reviewCount > 0 ? venueFeedbacks.Average(f => (double)f.Rating) : 0.0;
            ViewBag.Feedbacks = venueFeedbacks
                .Include(f => f.Booking.ClientUser)
                .OrderByDescending(f => f.CreatedAt)
                .Take(20)
                .ToList();

            return View(venue);
        }

        // GET: /Venue/GetVenueBookings?venueId
        [HttpGet]
        public JsonResult GetVenueBookings(int venueId)
        {
            // Lấy tất cả các Booking của sảnh này ngoại trừ các đơn đã bị Hủy (Status = 4)
            var bookings = db.Bookings
                .Where(b => b.VenueId == venueId && b.Status != 4)
                .Select(b => new { b.EventStartDateTime, b.EventEndDateTime })
                .ToList()
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