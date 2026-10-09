using EventVenueBooking.Database;
using EventVenueBooking.Entities;
using EventVenueBooking.Filters;
using EventVenueBooking.Services;
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace EventVenueBooking.Controllers
{
    // Phía Client: đặt sảnh, xem booking, thanh toán, hủy
    [CustomAuthorize(Roles = "Client")]
    public class ClientBookingController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // THÊM: số lượng tối đa của mỗi dịch vụ đi kèm trong một booking (chỉnh theo nhóm chốt)
        private const int MaxAddOnQuantity = 100;

        // UserId của người đang đăng nhập (ticket lưu Name = Email)
        private int CurrentUserId()
        {
            string email = User.Identity.Name;
            return db.Users.Where(u => u.Email == email).Select(u => u.UserId).FirstOrDefault();
        }

        // Chỉ lấy booking của chính người đang đăng nhập (người khác thì coi như không tồn tại)
        private Booking GetOwnedBooking(int id)
        {
            int userId = CurrentUserId();
            return db.Bookings
                .Include(b => b.Venue)
                .Include(b => b.EventType)
                .Include(b => b.Payments)
                .Include(b => b.BookingAddOns.Select(ba => ba.AddOnService))
                .FirstOrDefault(b => b.BookingId == id && b.ClientUserId == userId);
        }

        // Nạp dữ liệu hiển thị cho form; trả false nếu sảnh không tồn tại / không nhận đặt
        private bool FillForm(ClientBookingFormViewModel m)
        {
            var venue = db.Venues.Find(m.VenueId);
            if (venue == null || venue.Status != VenueStatuses.Active) return false;

            m.VenueName = venue.Name;
            m.RentalRate = venue.RentalRate;
            m.RentalUnit = venue.RentalUnit;
            m.Capacity = venue.Capacity;
            m.EventTypes = db.EventTypes.Where(e => e.IsActive).ToList();
            m.AvailableAddOns = db.AddOnServices.Where(a => a.IsActive)
                                  .OrderBy(a => a.Category).ThenBy(a => a.Name).ToList();
            return true;
        }

        // Kiểm tra dữ liệu đầu vào phía server. Trả về thông báo lỗi, hoặc null nếu hợp lệ.
        // SỬA: thêm kiểm tra số lượng dịch vụ đi kèm
        private string ValidateInput(int guestCount, int capacity, DateTime start, DateTime end,
                                     IEnumerable<AddOnRequest> addOns)
        {
            if (guestCount < 1)
                return "Số khách phải lớn hơn 0.";
            if (guestCount > capacity)
                return "Số khách tối đa là " + capacity + " người.";
            if (end <= start)
                return "Thời gian kết thúc phải sau thời gian bắt đầu.";
            if (start < DateTime.Now)
                return "Không thể đặt sảnh trong quá khứ.";
            if (addOns != null && addOns.Any(a => a.Quantity < 0 || a.Quantity > MaxAddOnQuantity))
                return "Số lượng dịch vụ đi kèm không hợp lệ (tối đa " + MaxAddOnQuantity + " mỗi dịch vụ).";
            return null;
        }

        // GET: ClientBooking/Create?venueId=1
        public ActionResult Create(int? venueId)
        {
            if (venueId == null) return RedirectToAction("Index", "Home");

            var model = new ClientBookingFormViewModel { VenueId = venueId.Value, GuestCount = 1 };
            if (!FillForm(model))
            {
                TempData["ErrorMessage"] = "Địa điểm không tồn tại hoặc hiện không nhận đặt.";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // POST: ClientBooking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ClientBookingFormViewModel model)
        {
            if (!FillForm(model))
            {
                TempData["ErrorMessage"] = "Địa điểm không tồn tại hoặc hiện không nhận đặt.";
                return RedirectToAction("Index", "Home");
            }

            if (model.EventStart == null || model.EventEnd == null)
            {
                ModelState.AddModelError("", "Vui lòng chọn thời gian bắt đầu và kết thúc.");
                return View(model);
            }

            // Kiểm tra số khách, thứ tự thời gian, quá khứ, số lượng add-on
            string error = ValidateInput(model.GuestCount, model.Capacity,
                                         model.EventStart.Value, model.EventEnd.Value, model.AddOns);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View(model);
            }

            try
            {
                var booking = new BookingService(db).CreateBooking(
                    CurrentUserId(), model.VenueId, model.EventTypeId, model.GuestCount,
                    model.EventStart.Value, model.EventEnd.Value, model.AddOns);

                TempData["BookingSuccess"] = "Đặt sảnh thành công! Vui lòng đặt cọc để xác nhận booking.";
                return RedirectToAction("Details", new { id = booking.BookingId });
            }
            catch (BookingException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }

        // POST (AJAX): tính giá tạm tính + kiểm tra trùng lịch, không lưu DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Quote(ClientBookingFormViewModel model)
        {
            try
            {
                if (model.EventStart == null || model.EventEnd == null)
                    return Json(new { ok = false, message = "Chọn thời gian bắt đầu và kết thúc." });

                var venue = db.Venues.Find(model.VenueId);
                if (venue == null)
                    return Json(new { ok = false, message = "Địa điểm không tồn tại." });

                // SỬA: sảnh không nhận đặt thì không báo giá
                if (venue.Status != VenueStatuses.Active)
                    return Json(new { ok = false, message = "Địa điểm hiện không nhận đặt." });

                string error = ValidateInput(model.GuestCount, venue.Capacity,
                                             model.EventStart.Value, model.EventEnd.Value, model.AddOns);
                if (error != null)
                    return Json(new { ok = false, message = error });

                var service = new BookingService(db);
                var quote = service.CalculatePrice(venue, model.EventStart.Value, model.EventEnd.Value, model.AddOns);
                bool taken = service.HasOverlap(venue.VenueId, model.EventStart.Value, model.EventEnd.Value);

                return Json(new
                {
                    ok = true,
                    available = !taken,
                    venueRate = quote.VenueRate,
                    rentalUnit = quote.RentalUnit,
                    rentalQuantity = quote.RentalQuantity,
                    venueCost = quote.VenueCost,
                    addOnCost = quote.AddOnCost,
                    total = quote.TotalCost,
                    // SỬA: làm tròn giống BookingService.RecordPayment để tiền cọc tạm tính khớp tiền cọc thật
                    deposit = Math.Round(quote.TotalCost * BookingService.DepositRate, 0, MidpointRounding.AwayFromZero),
                    lines = quote.AddOnLines.Select(l => new
                    {
                        name = l.Name,
                        quantity = l.Quantity,
                        unitPrice = l.UnitPrice,
                        subtotal = l.Subtotal
                    }).ToList()
                });
            }
            catch (BookingException ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
        }

        // GET: ClientBooking/Details/5
        public ActionResult Details(int id)
        {
            var booking = GetOwnedBooking(id);
            if (booking == null) return HttpNotFound();
            return View(booking);
        }

        // POST: ClientBooking/Pay/5  (paymentType: 0 = đặt cọc, 1 = thanh toán phần còn lại)
        // SỬA: bỏ danh sách phương thức riêng của controller, BookingService.RecordPayment kiểm tra tập trung
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pay(int id, byte paymentType, string method)
        {
            if (GetOwnedBooking(id) == null) return HttpNotFound();

            try
            {
                new BookingService(db).RecordPayment(id, paymentType, method, CurrentUserId());
                TempData["BookingSuccess"] = "Thanh toán thành công.";
            }
            catch (BookingException ex)
            {
                TempData["BookingError"] = ex.Message;
            }
            return RedirectToAction("Details", new { id });
        }

        // POST: ClientBooking/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id, string reason)
        {
            if (GetOwnedBooking(id) == null) return HttpNotFound();

            reason = string.IsNullOrWhiteSpace(reason) ? "Khách hàng hủy" : reason.Trim();
            if (reason.Length > 500) reason = reason.Substring(0, 500);

            try
            {
                new BookingService(db).CancelBooking(id, CurrentUserId(), reason);
                TempData["BookingSuccess"] = "Đã hủy booking.";
            }
            catch (BookingException ex)
            {
                TempData["BookingError"] = ex.Message;
            }
            return RedirectToAction("Details", new { id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}