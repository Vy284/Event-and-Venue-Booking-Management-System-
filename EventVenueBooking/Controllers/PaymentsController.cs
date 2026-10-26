using EventVenueBooking.Database;
using EventVenueBooking.Entities;
using EventVenueBooking.Filters;
using EventVenueBooking.Services;
using EventVenueBooking.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

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

            var viewModelList = payments.Select(p => new PaymentRowViewModel
            {
                PaymentId = p.PaymentId,
                BookingId = p.BookingId,
                CustomerName = p.Booking?.ClientUser?.FullName ?? "Unknown",
                Amount = p.Amount,
                PaymentType = p.PaymentType,
                PaymentMethod = string.IsNullOrEmpty(p.PaymentMethod) ? "N/A" : p.PaymentMethod,
                PaymentStatus = p.PaymentStatus,
                PaymentDate = p.PaymentDate,
                RecordedBy = p.RecordedByUser?.FullName ?? "System"
            }).ToList();

            ViewBag.TotalRevenue = payments
                .Where(p => p.PaymentStatus == PaymentStatuses.Completed)
                .Sum(p => (decimal?)p.Amount) ?? 0;
            ViewBag.TotalTransactions = payments.Count;
            ViewBag.PendingPayments = payments.Count(p => p.PaymentStatus == PaymentStatuses.Pending);
            ViewBag.RefundedPayments = payments.Count(p => p.PaymentStatus == PaymentStatuses.Refunded);

            return View("~/Views/Admin/Payments.cshtml", viewModelList);
        }

        // POST: Payments/Create  -> dùng lại BookingService.RecordPayment (tự tính tiền, tự Confirm booking khi có cọc)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int bookingId, byte paymentType, string paymentMethod)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Fail("Không xác định được tài khoản đang đăng nhập.");

            try
            {
                var payment = new BookingService(db)
                    .RecordPayment(bookingId, paymentType, paymentMethod, userId.Value);

                return Json(new
                {
                    success = true,
                    message = string.Format("Đã ghi nhận thanh toán #{0} ({1:N2}).", payment.PaymentId, payment.Amount)
                });
            }
            catch (BookingException ex)
            {
                return Fail(ex.Message);
            }
        }

        // POST: Payments/UpdateStatus  -> đổi phương thức, hoặc Pending -> Completed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, byte status, string method)
        {
            var payment = db.Payments.Find(id);
            if (payment == null)
                return Fail("Không tìm thấy giao dịch #" + id + ".");

            var booking = db.Bookings.Include(b => b.Payments)
                .FirstOrDefault(b => b.BookingId == payment.BookingId);
            if (booking == null)
                return Fail("Không tìm thấy booking của giao dịch này.");

            if (payment.PaymentStatus == PaymentStatuses.Refunded)
                return Fail("Giao dịch đã hoàn tiền, không thể chỉnh sửa.");
            if (status == PaymentStatuses.Refunded)
                return Fail("Để hoàn tiền hãy dùng nút Refund.");
            if (status > PaymentStatuses.Completed)
                return Fail("Trạng thái không hợp lệ.");
            if (payment.PaymentStatus == PaymentStatuses.Completed && status == PaymentStatuses.Pending)
                return Fail("Giao dịch đã Completed, không thể chuyển ngược về Pending.");

            var normalized = PaymentMethods.Normalize(method);
            if (normalized == null)
                return Fail("Phương thức thanh toán không hợp lệ.");

            // Pending -> Completed: kiểm tra giống RecordPayment
            if (payment.PaymentStatus == PaymentStatuses.Pending && status == PaymentStatuses.Completed)
            {
                if (booking.Status == BookingStatuses.Cancelled)
                    return Fail("Booking đã bị hủy.");
                if (booking.Status == BookingStatuses.Completed)
                    return Fail("Booking đã hoàn tất.");

                var others = booking.Payments
                    .Where(p => p.PaymentId != payment.PaymentId && p.PaymentStatus == PaymentStatuses.Completed)
                    .ToList();

                if (payment.PaymentType == PaymentTypes.Deposit &&
                    others.Any(p => p.PaymentType == PaymentTypes.Deposit))
                    return Fail("Booking này đã có khoản cọc hoàn thành.");

                if (payment.PaymentType == PaymentTypes.Final &&
                    !others.Any(p => p.PaymentType == PaymentTypes.Deposit))
                    return Fail("Cần đặt cọc trước khi hoàn thành khoản thanh toán cuối.");

                if (others.Sum(p => p.Amount) + payment.Amount > booking.TotalCost)
                    return Fail("Tổng thanh toán sẽ vượt tổng chi phí booking.");

                payment.PaymentStatus = PaymentStatuses.Completed;
                payment.PaymentDate = DateTime.Now;

                var userId = GetCurrentUserId();
                if (payment.PaymentType == PaymentTypes.Deposit && booking.Status == BookingStatuses.Pending)
                {
                    booking.Status = BookingStatuses.Confirmed;
                    booking.LastModifiedByUserId = userId;
                    booking.LastModifiedAt = DateTime.Now;
                }
            }

            payment.PaymentMethod = normalized;
            db.SaveChanges();
            return Json(new { success = true, message = "Đã cập nhật giao dịch #" + id + "." });
        }

        // POST: Payments/Refund
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Refund(int id)
        {
            var payment = db.Payments.Find(id);
            if (payment == null)
                return Fail("Không tìm thấy giao dịch #" + id + ".");

            if (payment.PaymentStatus == PaymentStatuses.Refunded)
                return Fail("Giao dịch này đã được hoàn tiền trước đó.");
            if (payment.PaymentStatus != PaymentStatuses.Completed)
                return Fail("Chỉ có thể hoàn tiền cho giao dịch đã Completed.");

            var booking = db.Bookings.Include(b => b.Payments)
                .FirstOrDefault(b => b.BookingId == payment.BookingId);
            if (booking == null)
                return Fail("Không tìm thấy booking của giao dịch này.");

            if (booking.Status == BookingStatuses.InProgress || booking.Status == BookingStatuses.Completed)
                return Fail("Booking đang diễn ra hoặc đã hoàn tất, không thể hoàn tiền từng khoản.");

            // Không cho hoàn cọc khi còn khoản Final đã thanh toán
            if (payment.PaymentType == PaymentTypes.Deposit &&
                booking.Payments.Any(p => p.PaymentType == PaymentTypes.Final &&
                                          p.PaymentStatus == PaymentStatuses.Completed))
                return Fail("Hãy hoàn khoản thanh toán cuối (Final) trước khi hoàn tiền cọc.");

            payment.PaymentStatus = PaymentStatuses.Refunded;

            // Hoàn cọc khi booking đang Confirmed -> trả về Pending để khớp rule "Confirmed phải có cọc"
            if (payment.PaymentType == PaymentTypes.Deposit && booking.Status == BookingStatuses.Confirmed)
            {
                booking.Status = BookingStatuses.Pending;
                booking.LastModifiedByUserId = GetCurrentUserId();
                booking.LastModifiedAt = DateTime.Now;
            }

            db.SaveChanges();
            return Json(new { success = true, message = "Đã hoàn tiền giao dịch #" + id + "." });
        }

        // ---------- helpers ----------
        private ActionResult Fail(string message)
        {
            return Json(new { success = false, message = message });
        }

        // GIẢ ĐỊNH: User.Identity.Name là Email. Nếu login lưu thứ khác thì sửa dòng Where.
        private int? GetCurrentUserId()
        {
            var name = User.Identity.Name;
            return db.Users
                .Where(u => u.Email == name)
                .Select(u => (int?)u.UserId)
                .FirstOrDefault();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}