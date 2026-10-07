using EventVenueBooking.Database;
using EventVenueBooking.Filters;                    // THÊM
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventVenueBooking.Controllers
{
    [CustomAuthorize(Roles = "Admin,Coordinator")]  // THÊM
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Dashboard
        public ActionResult Index()
        {
            var now = DateTime.Now;

            var model = new DashboardViewModel
            {
                // Thống kê số booking tháng này
                TotalBookingsThisMonth = db.Bookings
                    .Count(b => b.CreatedAt.Month == now.Month && b.CreatedAt.Year == now.Year),

                // Tổng doanh thu từ các booking không bị hủy
                TotalRevenue = db.Bookings
                    .Where(b => b.Status != 4) // 4 = Cancelled
                    .Select(b => (decimal?)b.TotalCost)
                    .Sum() ?? 0,

                // SỬA: Venue.Status 0 = Inactive, 1 = Active, 2 = UnderMaintenance
                // (bản cũ đếm Status == 0 nên đếm nhầm các địa điểm đang tắt)
                ActiveVenuesCount = db.Venues.Count(v => v.Status == 1),

                // Lấy 5 booking mới nhất
                RecentBookings = db.Bookings
                    .Include(b => b.ClientUser)
                    .Include(b => b.Venue)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .Select(b => new RecentBookingViewModel
                    {
                        BookingId = b.BookingId,
                        CustomerName = b.ClientUser.FullName,
                        VenueName = b.Venue.Name,
                        EventStartDateTime = b.EventStartDateTime,
                        TotalCost = b.TotalCost,
                        StatusText = b.Status == 0 ? "Pending" :
                                     b.Status == 1 ? "Confirmed" :
                                     b.Status == 2 ? "InProgress" :
                                     b.Status == 3 ? "Completed" : "Cancelled"
                    }).ToList()
            };

            return View("~/Views/Admin/Dashboard.cshtml", model);
        }



        public ActionResult Calendar(string weekDate)
        {
            DateTime today = DateTime.Today;

            // Tự parse chuỗi yyyy-MM-dd từ JS gửi lên
            if (!string.IsNullOrEmpty(weekDate))
            {
                DateTime.TryParse(weekDate, out today);
            }

            DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            DateTime endOfWeek = startOfWeek.AddDays(7);

            ViewBag.WeekStartDate = startOfWeek;

            // Các đoạn query db.Bookings ở dưới m GIỮ NGUYÊN...
            var bookings = db.Bookings
                .Include("Venue")
                .Where(b => b.EventStartDateTime >= startOfWeek && b.EventStartDateTime < endOfWeek)
                .ToList();

            var model = bookings.Select(b => new CalendarEventViewModel
            {
                BookingId = b.BookingId,
                VenueName = b.Venue != null ? b.Venue.Name : "N/A",
                StartDate = b.EventStartDateTime,
                EndDate = b.EventEndDateTime,
                EventStatus = GetStatusString(b.Status)
            }).ToList();

            return View("~/Views/Admin/Calendar.cshtml", model);
        }

        // Hàm phụ trợ map Status (byte) ra String theo thiết kế Booking.cs
        private string GetStatusString(byte statusByte)
        {
            switch (statusByte)
            {
                case 0: return "Pending";
                case 1: return "Confirmed";
                case 2: return "InProgress";
                case 3: return "Completed";
                case 4: return "Cancelled";
                default: return "Unknown";
            }
        }
    }
}