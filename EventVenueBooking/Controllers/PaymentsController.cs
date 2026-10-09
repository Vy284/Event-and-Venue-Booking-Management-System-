using EventVenueBooking.Database;
using EventVenueBooking.Filters;
using EventVenueBooking.Services;                   // THÊM: để dùng PaymentStatuses
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace EventVenueBooking.Controllers
{
    [CustomAuthorize(Roles = "Admin,Coordinator")]
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

            // Số liệu cho các thẻ thống kê (dùng hằng số, khớp với doanh thu ở Dashboard)
            ViewBag.TotalRevenue = payments
                .Where(p => p.PaymentStatus == PaymentStatuses.Completed)
                .Sum(p => (decimal?)p.Amount) ?? 0;
            ViewBag.TotalTransactions = payments.Count;
            ViewBag.PendingPayments = payments.Count(p => p.PaymentStatus == PaymentStatuses.Pending);
            ViewBag.RefundedPayments = payments.Count(p => p.PaymentStatus == PaymentStatuses.Refunded);

            return View("~/Views/Admin/Payments.cshtml", viewModelList);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}