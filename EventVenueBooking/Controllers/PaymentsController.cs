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
            var payments = db.Payments
                .Include(p => p.Booking)
                .Include(p => p.Booking.ClientUser)
                .Include(p => p.RecordedByUser)
                .OrderByDescending(p => p.PaymentDate ?? p.Booking.CreatedAt)
                .ToList();

            var viewModelList = payments.Select(p => new PaymentListViewModel
            {
                PaymentId = p.PaymentId,
                BookingId = p.BookingId,
                CustomerName = p.Booking?.ClientUser?.FullName ?? "Unknown",
                Amount = p.Amount,
                PaymentType = p.PaymentType, // 0 = Deposit, 1 = FinalPayment
                PaymentMethod = string.IsNullOrEmpty(p.PaymentMethod) ? "N/A" : p.PaymentMethod,
                PaymentStatus = p.PaymentStatus, // 0 = Pending, 1 = Completed, 2 = Refunded
                PaymentDate = p.PaymentDate,
                RecordedBy = p.RecordedByUser?.FullName ?? "System"
            }).ToList();

            // Tính toán nhanh cho các thẻ Thống kê (ViewBag)
            ViewBag.TotalRevenue = payments.Where(p => p.PaymentStatus == 1).Sum(p => (decimal?)p.Amount) ?? 0;
            ViewBag.TotalTransactions = payments.Count;
            ViewBag.PendingPayments = payments.Count(p => p.PaymentStatus == 0);
            ViewBag.RefundedPayments = payments.Count(p => p.PaymentStatus == 2);

            return View("~/Views/Admin/Payments.cshtml", viewModelList);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}