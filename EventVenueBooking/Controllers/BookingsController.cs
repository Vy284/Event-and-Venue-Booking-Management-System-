using EventVenueBooking.Database;
using EventVenueBooking.Entities;
using EventVenueBooking.Filters;                    // THÊM
using EventVenueBooking.Services;                   // THÊM
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
    [CustomAuthorize(Roles = "Admin,Coordinator")]  // THÊM
    public class BookingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // THÊM: lấy UserId người đang đăng nhập (ticket lưu Name = Email)
        private int CurrentUserId()
        {
            string email = User.Identity.Name;
            return db.Users.Where(u => u.Email == email).Select(u => u.UserId).First();
        }

        // THÊM: gom phần đổ dữ liệu dropdown cho view Create/Edit
        private void PopulateDropdowns(Booking booking)
        {
            ViewBag.CancelledByUserId = new SelectList(db.Users, "UserId", "FullName", booking?.CancelledByUserId);
            ViewBag.ClientUserId = new SelectList(db.Users, "UserId", "FullName", booking?.ClientUserId);
            ViewBag.EventTypeId = new SelectList(db.EventTypes, "EventTypeId", "TypeName", booking?.EventTypeId);
            ViewBag.LastModifiedByUserId = new SelectList(db.Users, "UserId", "FullName", booking?.LastModifiedByUserId);
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name", booking?.VenueId);
        }

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
                    Status = b.Status,                                                   // THÊM
                    PaidAmount = b.Payments                                              // THÊM
                        .Where(p => p.PaymentStatus == PaymentStatuses.Completed)
                        .Sum(p => p.Amount),
                    PaymentMethod = b.Payments.FirstOrDefault()?.PaymentMethod ?? "Chưa chọn",

                    // SỬA: "Paid" chỉ khi đã thu đủ; mới cọc thì "Partial"
                    PaymentStatus =
                        b.Payments.Where(p => p.PaymentStatus == PaymentStatuses.Completed).Sum(p => p.Amount) >= b.TotalCost
                            ? "Paid"
                            : (b.Payments.Any(p => p.PaymentStatus == PaymentStatuses.Completed) ? "Partial" : "Unpaid"),

                    // Map danh sách tên dịch vụ đi kèm vào ViewModel
                    AddOnServices = b.BookingAddOns != null
                        ? b.BookingAddOns.Select(ba => ba.AddOnService.Name).ToList()
                        : new List<string>()
                }).ToList();

            // THÊM: số liệu cho 4 thẻ thống kê (trước đây là số cố định trong view)
            ViewBag.PendingCount = bookings.Count(x => x.Status == BookingStatuses.Pending);
            ViewBag.ConfirmedCount = bookings.Count(x => x.Status == BookingStatuses.Confirmed || x.Status == BookingStatuses.InProgress);
            ViewBag.CompletedCount = bookings.Count(x => x.Status == BookingStatuses.Completed);
            ViewBag.CancelledCount = bookings.Count(x => x.Status == BookingStatuses.Cancelled);

            return View("~/Views/Admin/Bookings.cshtml", bookings);
        }

        // GET: Details
        public ActionResult Details(int? id)
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

        // GET: Create
        public ActionResult Create()
        {
            PopulateDropdowns(null);                // SỬA: dùng hàm gom
            return View();
        }

        // POST: Create
        // SỬA: không bind cả Booking nữa. Chỉ nhận các ô nhập, giá và trạng thái do BookingService tính.
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
            PopulateDropdowns(booking);             // SỬA: dùng hàm gom
            return View(booking);
        }

        // POST: Edit
        // SỬA: chỉ cho đổi trạng thái (qua BookingService). Giá, venue, thời gian không sửa ở đây
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

        // THÊM: Đổi trạng thái (nút Bắt đầu / Hoàn tất trên trang danh sách)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeStatus(int id, byte status)
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
            return RedirectToAction("Index");
        }

        // THÊM: Ghi nhận thanh toán, paymentType: 0 = Deposit, 1 = FinalPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecordPayment(int id, byte paymentType, string method)
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
            return RedirectToAction("Index");
        }

        // THÊM: Hủy booking kèm lý do (modal Cancel ở trang danh sách)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id, string reason)
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
            return RedirectToAction("Index");
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

        // POST: Delete  (không xóa thật, chỉ hủy)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }

            // SỬA: hủy qua service để ghi người hủy, lý do, và chuyển khoản đã thu sang Refunded
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