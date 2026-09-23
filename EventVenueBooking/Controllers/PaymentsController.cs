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
    public class PaymentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Payments
        public ActionResult Index()
        {
            // Tái sử dụng BookingManagementViewModel để quản lý bảng thanh toán
            var paymentsList = db.Bookings
                .Include(b => b.ClientUser)
                .Include(b => b.Venue)
                .Include(b => b.Payments)
                .OrderByDescending(b => b.CreatedAt)
                .ToList()
                .Select(b => new BookingManagementViewModel
                {
                    BookingId = b.BookingId,
                    CustomerName = b.ClientUser?.FullName ?? "N/A",
                    VenueName = b.Venue?.Name ?? "N/A",
                    BookingDate = b.EventStartDateTime,
                    TotalAmount = b.TotalCost,
                    PaymentMethod = b.Payments.FirstOrDefault()?.PaymentMethod ?? "Tiền mặt / Chuyển khoản",
                    PaymentStatus = b.Payments.Any(p => p.PaymentStatus == 1) ? "Paid" : "Unpaid"
                }).ToList();

            return View(paymentsList);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}