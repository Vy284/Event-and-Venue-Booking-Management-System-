using EventVenueBooking.Database;
using EventVenueBooking.Filters;
using EventVenueBooking.Services;
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventVenueBooking.Controllers
{
    [CustomAuthorize(Roles = "Admin,Coordinator")]
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Dashboard/Index
        public ActionResult Index()
        {
            var now = DateTime.Now;

            var model = new DashboardViewModel
            {
                TotalBookingsThisMonth = db.Bookings
                    .Count(b => b.CreatedAt.Month == now.Month && b.CreatedAt.Year == now.Year),

                TotalRevenue = db.Payments
                    .Where(p => p.PaymentStatus == PaymentStatuses.Completed)
                    .Select(p => (decimal?)p.Amount)
                    .Sum() ?? 0,

                ActiveVenuesCount = db.Venues.Count(v => v.Status == 1),

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