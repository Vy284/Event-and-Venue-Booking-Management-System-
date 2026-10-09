using EventVenueBooking.Database;
using EventVenueBooking.Entities;
using EventVenueBooking.Filters;
using EventVenueBooking.Services;
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EventVenueBooking.Controllers
{
    [CustomAuthorize(Roles = "Admin,Coordinator")]
    public class BookingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Lấy UserId người đang đăng nhập (ticket lưu Name = Email)
        private int CurrentUserId()
        {
            string email = User.Identity.Name;
            return db.Users.Where(u => u.Email == email).Select(u => u.UserId).First();
        }

        // SỬA: quay về trang chi tiết nếu form gửi từ đó, ngược lại về danh sách
        private ActionResult BackTo(string returnTo, int id)
        {
            if (returnTo == "details")
                return RedirectToAction("Details", new { id });
            return RedirectToAction("Index");
        }

        // Đổ dữ liệu dropdown cho view Create/Edit
        // SỬA 2.1: chỉ liệt kê tài khoản Client (Role = 0) đang hoạt động
        // SỬA 2.3: chỉ liệt kê EventType và Venue đang hoạt động
        // SỬA 2.2: bỏ dropdown CancelledByUserId / LastModifiedByUserId (người thao tác do server tự ghi)
        private void PopulateDropdowns(Booking booking)
        {
            ViewBag.ClientUserId = new SelectList(
                db.Users.Where(u => u.Role == 0 && u.IsActive),
                "UserId", "FullName", booking?.ClientUserId);

            ViewBag.EventTypeId = new SelectList(
                db.EventTypes.Where(e => e.IsActive),
                "EventTypeId", "TypeName", booking?.EventTypeId);

            ViewBag.VenueId = new SelectList(
                db.Venues.Where(v => v.Status == VenueStatuses.Active),
                "VenueId", "Name", booking?.VenueId);
        }

        // GET: Bookings
        public ActionResult Index()
        {
            var bookings = db.Bookings
                .Include(b => b.ClientUser)
                .Include(b => b.Venue)
                .Include(b => b.Payments)
                .Include(b => b.BookingAddOns.Select(ba => ba.AddOnService))
                .OrderByDescending(b => b.CreatedAt)
                .ToList()
                .Select(b => new BookingManagementViewModel
                {
                    BookingId = b.BookingId,
                    CustomerName = b.ClientUser != null ? b.ClientUser.FullName : "N/A",
                    VenueName = b.Venue != null ? b.Venue.Name : "N/A",
                    BookingDate = b.EventStartDateTime,
                    TotalAmount = b.TotalCost,
                    Status = b.Status,
                    PaidAmount = b.Payments
                        .Where(p => p.PaymentStatus == PaymentStatuses.Completed)
                        .Sum(p => p.Amount),
                    PaymentMethod = b.Payments.FirstOrDefault()?.PaymentMethod ?? "Chưa chọn",

                    // "Paid" chỉ khi đã thu đủ; mới cọc thì "Partial"
                    PaymentStatus =
                        b.Payments.Where(p => p.PaymentStatus == PaymentStatuses.Completed).Sum(p => p.Amount) >= b.TotalCost
                            ? "Paid"
                            : (b.Payments.Any(p => p.PaymentStatus == PaymentStatuses.Completed) ? "Partial" : "Unpaid"),

                    // Map danh sách tên dịch vụ đi kèm vào ViewModel
                    AddOnServices = b.BookingAddOns != null
                        ? b.BookingAddOns.Select(ba => ba.AddOnService.Name).ToList()
                        : new List<string>()
                }).ToList();

            // Số liệu cho 4 thẻ thống kê
            ViewBag.PendingCount = bookings.Count(x => x.Status == BookingStatuses.Pending);
            ViewBag.ConfirmedCount = bookings.Count(x => x.Status == BookingStatuses.Confirmed || x.Status == BookingStatuses.InProgress);
            ViewBag.CompletedCount = bookings.Count(x => x.Status == BookingStatuses.Completed);
            ViewBag.CancelledCount = bookings.Count(x => x.Status == BookingStatuses.Cancelled);

            return View("~/Views/Admin/Bookings.cshtml", bookings);
        }

        // GET: Details
        // SỬA: nạp sẵn khách, địa điểm, loại sự kiện, thanh toán, add-on (bản cũ dùng Find nên các mục này bị null)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Booking booking = db.Bookings
                .Include(b => b.ClientUser)
                .Include(b => b.Venue)
                .Include(b => b.EventType)
                .Include(b => b.LastModifiedByUser)
                .Include(b => b.CancelledByUser)
                .Include(b => b.Payments)
                .Include(b => b.BookingAddOns.Select(ba => ba.AddOnService))
                .FirstOrDefault(b => b.BookingId == id);

            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // GET: Create
        public ActionResult Create()
        {
            PopulateDropdowns(null);
            return View();
        }

        // POST: Create
        // Không bind cả Booking. Chỉ nhận các ô nhập, giá và trạng thái do BookingService tính.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int clientUserId, int venueId, int eventTypeId, int guestCount,
                                   DateTime eventStartDateTime, DateTime eventEndDateTime)
        {
            try
            {
                new BookingService(db).CreateBooking(
                    clientUserId, venueId, eventTypeId, guestCount,
                    eventStartDateTime, eventEndDateTime, null);

                TempData["SuccessMessage"] = "Tạo booking thành công.";
                return RedirectToAction("Index");
            }
            catch (BookingException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            // Lỗi: hiện lại form với dữ liệu vừa nhập
            var model = new Booking
            {
                ClientUserId = clientUserId,
                VenueId = venueId,
                EventTypeId = eventTypeId,
                GuestCount = guestCount,
                EventStartDateTime = eventStartDateTime,
                EventEndDateTime = eventEndDateTime
            };
            PopulateDropdowns(model);
            return View(model);
        }

        // GET: Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            PopulateDropdowns(booking);
            return View(booking);
        }

        // POST: Edit
        // Chỉ cho đổi trạng thái (qua BookingService). Giá, venue, thời gian không sửa ở đây
        // để giữ nguyên giá tại thời điểm đặt và không lọt trùng lịch.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int bookingId, byte status)
        {
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            try
            {
                if (status != booking.Status)
                {
                    var service = new BookingService(db);
                    if (status == BookingStatuses.Cancelled)
                        service.CancelBooking(bookingId, CurrentUserId(), "Hủy bởi quản trị");
                    else
                        service.ChangeStatus(bookingId, status, CurrentUserId());
                }
                return RedirectToAction("Index");
            }
            catch (BookingException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            db.Entry(booking).Reload();             // bỏ thay đổi dở dang, lấy lại dữ liệu thật
            PopulateDropdowns(booking);
            return View(booking);
        }

        // Đổi trạng thái (nút Bắt đầu / Hoàn tất trên danh sách, và nút trên trang chi tiết)
        // SỬA: thêm returnTo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeStatus(int id, byte status, string returnTo = null)
        {
            try
            {
                new BookingService(db).ChangeStatus(id, status, CurrentUserId());
                TempData["SuccessMessage"] = "Đã cập nhật trạng thái booking.";
            }
            catch (BookingException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return BackTo(returnTo, id);
        }

        // Ghi nhận thanh toán, paymentType: 0 = Deposit, 1 = FinalPayment
        // SỬA: thêm returnTo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecordPayment(int id, byte paymentType, string method, string returnTo = null)
        {
            try
            {
                new BookingService(db).RecordPayment(id, paymentType, method, CurrentUserId());
                TempData["SuccessMessage"] = "Đã ghi nhận thanh toán.";
            }
            catch (BookingException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return BackTo(returnTo, id);
        }

        // Hủy booking kèm lý do (modal Cancel ở trang danh sách, và form trên trang chi tiết)
        // SỬA: thêm returnTo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id, string reason, string returnTo = null)
        {
            reason = string.IsNullOrWhiteSpace(reason) ? "Hủy bởi quản trị" : reason.Trim();
            if (reason.Length > 500) reason = reason.Substring(0, 500);

            try
            {
                new BookingService(db).CancelBooking(id, CurrentUserId(), reason);
                TempData["SuccessMessage"] = "Đã hủy booking.";
            }
            catch (BookingException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return BackTo(returnTo, id);
        }

        // GET: Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Delete (không xóa thật, chỉ hủy)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }

            // Hủy qua service để ghi người hủy, lý do, và chuyển khoản đã thu sang Refunded
            try
            {
                new BookingService(db).CancelBooking(id, CurrentUserId(), "Hủy bởi quản trị");
                TempData["SuccessMessage"] = "Đã hủy booking.";
            }
            catch (BookingException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}