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

        // Lấy UserId người đang đăng nhập
        private int CurrentUserId()
        {
            string email = User.Identity.Name;
            return db.Users.Where(u => u.Email == email).Select(u => u.UserId).First();
        }

        // Quay về trang chi tiết nếu form gửi từ đó, ngược lại về danh sách
        private ActionResult BackTo(string returnTo, int id)
        {
            if (returnTo == "details")
                return RedirectToAction("Details", new { id });
            return RedirectToAction("Index");
        }

        // Đổ dữ liệu dropdown cho View Create/Edit
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

                    PaymentStatus =
                        b.Payments.Where(p => p.PaymentStatus == PaymentStatuses.Completed).Sum(p => p.Amount) >= b.TotalCost
                            ? "Paid"
                            : (b.Payments.Any(p => p.PaymentStatus == PaymentStatuses.Completed) ? "Partial" : "Unpaid"),

                    AddOnServices = b.BookingAddOns != null
                        ? b.BookingAddOns.Select(ba => ba.AddOnService.Name).ToList()
                        : new List<string>()
                }).ToList();

            // Số liệu cho 4 thẻ thống kê
            ViewBag.PendingCount = bookings.Count(x => x.Status == BookingStatuses.Pending);
            ViewBag.ConfirmedCount = bookings.Count(x => x.Status == BookingStatuses.Confirmed || x.Status == BookingStatuses.InProgress);
            ViewBag.CompletedCount = bookings.Count(x => x.Status == BookingStatuses.Completed);
            ViewBag.CancelledCount = bookings.Count(x => x.Status == BookingStatuses.Cancelled);

            // Nạp danh sách Add-on cho checkbox trong popup New Booking của Admin
            ViewBag.AddOnServices = db.AddOnServices.Where(a => a.IsActive)
                .ToList()
                .Select(a => new SelectListItem
                {
                    Value = a.AddOnId.ToString(),
                    Text = a.Name + " (" + a.Price.ToString("N0") + " VNĐ)"
                })
                .ToList();

            PopulateDropdowns(null);

            return View("~/Views/Admin/Bookings.cshtml", bookings);
        }

        // GET: Details
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

        // POST: Create (popup Admin) - chọn sảnh/loại sự kiện bằng dropdown, có giờ kết thúc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int clientUserId, int venueId, int eventTypeId, int guestCount,
                                   DateTime eventStartDateTime, DateTime eventEndDateTime,
                                   List<int> SelectedAddOnIds)
        {
            try
            {
                if (eventEndDateTime <= eventStartDateTime)
                    throw new BookingException("Thời gian kết thúc phải sau thời gian bắt đầu.");

                List<AddOnRequest> addOnRequests = null;
                if (SelectedAddOnIds != null && SelectedAddOnIds.Any())
                {
                    addOnRequests = SelectedAddOnIds.Select(id => new AddOnRequest
                    {
                        AddOnId = id,
                        Quantity = 1
                    }).ToList();
                }

                new BookingService(db).CreateBooking(
                    clientUserId, venueId, eventTypeId, guestCount,
                    eventStartDateTime, eventEndDateTime, addOnRequests);

                TempData["SuccessMessage"] = "Tạo booking thành công.";
            }
            catch (BookingException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống, vui lòng thử lại.";
            }

            return RedirectToAction("Index");
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

            db.Entry(booking).Reload();
            PopulateDropdowns(booking);
            return View(booking);
        }

        // Đổi trạng thái (Đã được bổ sung kiểm tra bắt buộc thanh toán tiền cọc)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeStatus(int id, byte status, string returnTo = null)
        {
            try
            {
                var booking = db.Bookings.Include(b => b.Payments).FirstOrDefault(b => b.BookingId == id);
                if (booking == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra nghiệp vụ: Nếu chuyển từ Pending sang Confirmed thủ công, bắt buộc phải có tiền cọc hoàn thành
                if (booking.Status == BookingStatuses.Pending && status == BookingStatuses.Confirmed)
                {
                    bool hasCompletedDeposit = booking.Payments != null && booking.Payments.Any(p =>
                        p.PaymentType == PaymentTypes.Deposit &&
                        p.PaymentStatus == PaymentStatuses.Completed);

                    if (!hasCompletedDeposit)
                    {
                        throw new BookingException("Không thể chuyển sang trạng thái Đã xác nhận (Confirmed) vì khách hàng chưa hoàn thành thanh toán tiền cọc.");
                    }
                }

                new BookingService(db).ChangeStatus(id, status, CurrentUserId());
                TempData["SuccessMessage"] = "Đã cập nhật trạng thái booking.";
            }
            catch (BookingException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return BackTo(returnTo, id);
        }

        // Ghi nhận thanh toán
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

        // Hủy booking
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

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }

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

        // XUẤT HỢP ĐỒNG DƯỚI DẠNG WORD (.doc) ĐỂ TRÁNH LỖI MỞ FILE PDF THÔ
        [HttpGet]
        public ActionResult ExportContractPdf(int id)
        {
            var booking = db.Bookings
                .Include(b => b.ClientUser)
                .Include(b => b.Venue)
                .Include(b => b.EventType)
                .Include(b => b.BookingAddOns.Select(ba => ba.AddOnService))
                .FirstOrDefault(b => b.BookingId == id);

            if (booking == null)
            {
                return HttpNotFound("Không tìm thấy thông tin đặt chỗ để xuất hợp đồng.");
            }

            string htmlContent = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8' />
                    <style>
                        body {{ font-family: 'Times New Roman', Times, serif; padding: 30px; color: #000; font-size: 14px; line-height: 1.6; }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .company-title {{ font-size: 16px; font-weight: bold; text-transform: uppercase; }}
                        .contract-title {{ font-size: 20px; font-weight: bold; text-transform: uppercase; margin-top: 10px; }}
                        .section-title {{ font-weight: bold; margin-top: 15px; text-decoration: underline; }}
                        .info-row {{ margin-bottom: 6px; }}
                        .footer-table {{ width: 100%; margin-top: 50px; border: none; }}
                        .signature {{ text-align: center; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <div class='company-title'>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div>
                        <div style='font-size: 13px; font-weight: bold;'>Độc lập - Tự do - Hạnh phúc</div>
                        <hr style='width: 200px; margin: 10px auto;'/>
                        <div class='contract-title'>HỢP ĐỒNG DỊCH VỤ TỔ CHỨC SỰ KIỆN & ĐẶT SẢNH</div>
                        <div>Mã hợp đồng: #BK-{booking.BookingId} | Ngày lập: {DateTime.Now:dd/MM/yyyy}</div>
                    </div>

                    <div class='section-title'>I. BÊN CUNG CẤP DỊCH VỤ (TRUNG TÂM SẢNH SỰ KIỆN)</div>
                    <div class='info-row'>- Đơn vị: Hệ thống Quản lý Sảnh Sự Kiện EventVenue</div>
                    <div class='info-row'>- Hotline hỗ trợ: 1900 xxxx</div>

                    <div class='section-title' style='margin-top: 15px;'>II. BÊN SỬ DỤNG DỊCH VỤ (KHÁCH HÀNG)</div>
                    <div class='info-row'>- Họ và tên khách hàng: <strong>{HttpUtility.HtmlEncode(booking.ClientUser != null ? booking.ClientUser.FullName : "N/A")}</strong></div>
                    <div class='info-row'>- Email: {HttpUtility.HtmlEncode(booking.ClientUser != null ? booking.ClientUser.Email : "N/A")}</div>
                    <div class='info-row'>- Số điện thoại: {HttpUtility.HtmlEncode(booking.ClientUser != null && !string.IsNullOrEmpty(booking.ClientUser.Phone) ? booking.ClientUser.Phone : "N/A")}</div>

                    <div class='section-title' style='margin-top: 15px;'>III. NỘI DUNG VÀ CHI PHÍ ĐẶT SẢNH</div>
                    <div class='info-row'>- Sảnh sự kiện: <strong>{HttpUtility.HtmlEncode(booking.Venue != null ? booking.Venue.Name : "N/A")}</strong></div>
                    <div class='info-row'>- Vị trí / Địa điểm: {HttpUtility.HtmlEncode(booking.Venue != null ? booking.Venue.Location : "N/A")}</div>
                    <div class='info-row'>- Loại sự kiện: {HttpUtility.HtmlEncode(booking.EventType != null ? booking.EventType.TypeName : "N/A")}</div>
                    <div class='info-row'>- Thời gian bắt đầu: <strong>{booking.EventStartDateTime:dd/MM/yyyy HH:mm}</strong></div>
                    <div class='info-row'>- Thời gian kết thúc: <strong>{booking.EventEndDateTime:dd/MM/yyyy HH:mm}</strong></div>
                    <div class='info-row'>- Số lượng khách tham dự: {booking.GuestCount} người</div>
                    
                    <div class='info-row' style='margin-top: 10px;'><strong>Tổng giá trị hợp đồng:</strong> <span style='color: red; font-size: 16px; font-weight: bold;'>{booking.TotalCost:N0} VNĐ</span></div>
                    <div class='info-row'>- Trạng thái đơn đặt: {(new[] { "Chờ đặt cọc", "Đã xác nhận", "Đang diễn ra", "Hoàn thành", "Đã hủy" })[booking.Status]}</div>

                    <table class='footer-table'>
                        <tr>
                            <td class='signature' style='width: 50%;'>
                                <p>ĐẠI DIỆN TRUNG TÂM</p>
                                <p style='font-size: 11px; font-weight: normal;'>(Ký, đóng dấu)</p>
                                <br/><br/><br/>
                            </td>
                            <td class='signature' style='width: 50%;'>
                                <p>ĐẠI DIỆN KHÁCH HÀNG</p>
                                <p style='font-size: 11px; font-weight: normal;'>(Ký, ghi rõ họ tên)</p>
                                <br/><br/><br/>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(htmlContent);
            return File(byteArray, "application/msword", $"HopDong_Booking_{booking.BookingId}.doc");
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