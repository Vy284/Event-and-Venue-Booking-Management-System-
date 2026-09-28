using EventVenueBooking.Database;
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace EventVenueBooking.Controllers
{
    public class BookingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Bookings
        public ActionResult Index()
        {
            var bookings = db.Bookings
                .Include(b => b.ClientUser)
                .Include(b => b.Venue)
                .Include(b => b.Payments)
                .Include(b => b.BookingAddOns.Select(ba => ba.AddOnService)) // Include thêm bảng dịch vụ đi kèm nếu có quan hệ
                .OrderByDescending(b => b.CreatedAt)
                .ToList()
                .Select(b => new BookingManagementViewModel
                {
                    BookingId = b.BookingId,
                    CustomerName = b.ClientUser != null ? b.ClientUser.FullName : "N/A",
                    VenueName = b.Venue != null ? b.Venue.Name : "N/A",
                    BookingDate = b.EventStartDateTime,
                    TotalAmount = b.TotalCost,
                    PaymentMethod = b.Payments.FirstOrDefault()?.PaymentMethod ?? "Chưa chọn",
                    PaymentStatus = b.Payments.Any(p => p.PaymentStatus == 1) ? "Paid" : "Unpaid",

                    // Map danh sách tên dịch vụ đi kèm vào ViewModel
                    AddOnServices = b.BookingAddOns != null
                        ? b.BookingAddOns.Select(ba => ba.AddOnService.Name).ToList()
                        : new List<string>()
                }).ToList();

            return View("~/Views/Admin/Bookings.cshtml", bookings);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}