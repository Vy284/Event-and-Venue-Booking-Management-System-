using EventVenueBooking.Database;
using EventVenueBooking.Filters;
using EventVenueBooking.Services;
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EventVenueBooking.Controllers
{
    // Cả Admin và Coordinator vào được controller này (Calendar); Index và Export chỉ dành cho Admin
    [CustomAuthorize(Roles = "Admin,Coordinator")]
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Dashboard/Index  (chỉ Admin)
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var now = DateTime.Now;
            var model = new AdminDashboardViewModel();

            // ---- 3 thẻ thống kê ----
            model.TotalBookings = db.Bookings.Count();

            model.UpcomingEvents = db.Bookings.Count(b =>
                b.Status != BookingStatuses.Cancelled && b.EventStartDateTime >= now);

            model.TotalRevenue = db.Payments
                .Where(p => p.PaymentStatus == PaymentStatuses.Completed)
                .Select(p => (decimal?)p.Amount)
                .Sum() ?? 0;

            // ---- Trạng thái booking (0..4) ----
            var counts = db.Bookings
                .GroupBy(b => b.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            string[] statusNames = { "Pending", "Confirmed", "In Progress", "Completed", "Cancelled" };
            string[] statusColors = { "#F7C17F", "#184A3B", "#5FA58C", "#8FA3E8", "#DCE0FA" };

            for (int i = 0; i < statusNames.Length; i++)
            {
                int c = counts.Where(x => x.Status == i).Select(x => x.Count).FirstOrDefault();
                model.StatusCounts.Add(new DashboardStatusItem
                {
                    Name = statusNames[i],
                    Count = c,
                    Percent = model.TotalBookings > 0 ? Math.Round(100.0 * c / model.TotalBookings) : 0,
                    Color = statusColors[i]
                });
            }

            // ---- Doanh thu 8 tháng gần nhất: giá thuê sảnh vs dịch vụ đi kèm (booking chưa hủy) ----
            var firstMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-7);

            var monthly = db.Bookings
                .Where(b => b.Status != BookingStatuses.Cancelled && b.CreatedAt >= firstMonth)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Venue = g.Sum(x => x.VenueCost),
                    AddOn = g.Sum(x => x.AddOnCost)
                })
                .ToList();

            for (int i = 0; i < 8; i++)
            {
                var m = firstMonth.AddMonths(i);
                var row = monthly.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month);

                decimal venueCost = row != null ? row.Venue : 0;
                decimal addOnCost = row != null ? row.AddOn : 0;

                model.RevenueLabels.Add(m.ToString("MM/yyyy"));
                model.VenueCostSeries.Add(venueCost);
                model.AddOnCostSeries.Add(addOnCost);
                model.RevenueSeriesTotal += venueCost + addOnCost;
            }

            // ---- Top loại sự kiện (booking chưa hủy) ----
            int activeTotal = db.Bookings.Count(b => b.Status != BookingStatuses.Cancelled);

            var topEvents = db.Bookings
                .Where(b => b.Status != BookingStatuses.Cancelled)
                .GroupBy(b => b.EventType.TypeName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Name)
                .Take(5)
                .ToList();

            foreach (var t in topEvents)
            {
                model.TopEventTypes.Add(new DashboardEventTypeItem
                {
                    Name = t.Name,
                    Count = t.Count,
                    Percent = activeTotal > 0 ? Math.Round(100.0 * t.Count / activeTotal) : 0
                });
            }

            return View("~/Views/Admin/Dashboard.cshtml", model);
        }

        // GET: Dashboard/Calendar
        public ActionResult Calendar(string weekDate)
        {
            DateTime startDate;

            if (!string.IsNullOrEmpty(weekDate) && DateTime.TryParse(weekDate, out DateTime parsed))
            {
                startDate = parsed.Date;
            }
            else
            {
                var today = DateTime.Today;
                startDate = new DateTime(today.Year, today.Month, ((today.Day - 1) / 7) * 7 + 1);
            }

            DateTime endDate = startDate.AddDays(7);
            ViewBag.WeekStartDate = startDate;

            var bookings = db.Bookings
                .Include("Venue")
                .Where(b => b.Status != BookingStatuses.Cancelled
                         && b.EventStartDateTime >= startDate
                         && b.EventStartDateTime < endDate)
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

        // GET: Dashboard/ExportRevenueExcel (Xuất file Excel doanh thu, chỉ Admin)
        [HttpGet]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ExportRevenueExcel()
        {
            var payments = db.Payments
                .Where(p => p.PaymentStatus == PaymentStatuses.Completed)
                .Include(p => p.Booking.ClientUser)
                .Include(p => p.Booking.Venue)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            var sb = new StringBuilder();
            sb.Append("<table border='1'>");
            sb.Append("<tr style='background-color:#f2f2f2; font-weight:bold;'>");
            sb.Append("<th>Mã Thanh Toán</th>");
            sb.Append("<th>Khách Hàng</th>");
            sb.Append("<th>Sảnh Đặt</th>");
            sb.Append("<th>Số Tiền (VNĐ)</th>");
            sb.Append("<th>Ngày Thanh Toán</th>");
            sb.Append("</tr>");

            foreach (var p in payments)
            {
                string customerName = p.Booking != null && p.Booking.ClientUser != null ? p.Booking.ClientUser.FullName : "N/A";
                string venueName = p.Booking != null && p.Booking.Venue != null ? p.Booking.Venue.Name : "N/A";
                string paymentDate = p.PaymentDate.HasValue ? p.PaymentDate.Value.ToString("dd/MM/yyyy HH:mm") : "";

                sb.Append("<tr>");
                sb.Append("<td>").Append(p.PaymentId).Append("</td>");
                sb.Append("<td>").Append(HttpUtility.HtmlEncode(customerName)).Append("</td>");
                sb.Append("<td>").Append(HttpUtility.HtmlEncode(venueName)).Append("</td>");
                sb.Append("<td>").Append(p.Amount.ToString("0")).Append("</td>");
                sb.Append("<td>").Append(paymentDate).Append("</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            // \uFEFF: BOM để Excel hiển thị tiếng Việt UTF-8 đúng
            byte[] bytes = Encoding.UTF8.GetBytes("\uFEFF" + sb.ToString());
            string fileName = "DoanhThu_EventVenue_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xls";

            return File(bytes, "application/vnd.ms-excel", fileName);
        }

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

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}