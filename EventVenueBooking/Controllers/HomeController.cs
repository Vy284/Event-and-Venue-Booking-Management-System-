using EventVenueBooking.Database;
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventVenueBooking.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index(string search, string category, string eventDate, int? maxCapacity, decimal? maxPrice)
        {
            // Venue.Status 0 = Inactive, 1 = Active, 2 = UnderMaintenance. Chỉ hiện sảnh Active.
            var query = db.Venues.Where(v => v.Status == 1).AsQueryable();

            // Lọc theo Tên sảnh hoặc Vị trí
            if (!string.IsNullOrEmpty(search))
            {
                string searchLower = search.Trim().ToLower();
                query = query.Where(v => v.Name.ToLower().Contains(searchLower) || v.Location.ToLower().Contains(searchLower));
            }

            // Lọc theo Category dựa trên VenueType và EventType
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                string categoryLower = category.Trim().ToLower();

                if (categoryLower == "wedding")
                {
                    query = query.Where(v => v.VenueType.TypeName.ToLower().Contains("ballroom")
                                          || v.VenueType.TypeName.ToLower().Contains("garden")
                                          || v.Name.ToLower().Contains("wedding"));
                }
                else if (categoryLower == "corporate")
                {
                    query = query.Where(v => v.VenueType.TypeName.ToLower().Contains("ballroom")
                                          || v.VenueType.TypeName.ToLower().Contains("indoor")
                                          || v.Name.ToLower().Contains("corporate"));
                }
                else if (categoryLower == "birthday")
                {
                    query = query.Where(v => v.VenueType.TypeName.ToLower().Contains("rooftop")
                                          || v.VenueType.TypeName.ToLower().Contains("garden"));
                }
                else if (categoryLower == "social")
                {
                    query = query.Where(v => v.VenueType.TypeName.ToLower().Contains("rooftop")
                                          || v.VenueType.TypeName.ToLower().Contains("lounge"));
                }
                else
                {
                    query = query.Where(v => v.VenueType.TypeName.ToLower().Contains(category.ToLower()));
                }
            }

            // Lọc theo số khách: chỉ giữ sảnh chứa được ít nhất ngần ấy người
            if (maxCapacity.HasValue && maxCapacity.Value > 0)
            {
                int cap = maxCapacity.Value;
                query = query.Where(v => v.Capacity >= cap);
            }

            // Lọc theo giá tối đa
            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                decimal price = maxPrice.Value;
                query = query.Where(v => v.RentalRate <= price);
            }

            // Lọc tìm theo ngày
            if (!string.IsNullOrEmpty(eventDate) && DateTime.TryParse(eventDate, out DateTime selectedDate))
            {
                DateTime startOfDay = selectedDate.Date;
                DateTime endOfDay = startOfDay.AddDays(1);

                // Loại bỏ những sảnh đã có Booking vướng vào ngày này (Status != 4)
                query = query.Where(v => !v.Bookings.Any(b =>
                    b.Status != 4 &&
                    b.EventStartDateTime < endOfDay &&
                    b.EventEndDateTime > startOfDay
                ));
            }

            var venues = query.Select(v => new VenueCardViewModel
            {
                VenueId = v.VenueId,
                Name = v.Name,
                TypeName = v.VenueType.TypeName,
                Location = v.Location,
                Capacity = v.Capacity,
                RentalRate = v.RentalRate,
                RentalUnit = v.RentalUnit,
                ImageUrl = db.VenueImages.FirstOrDefault(img => img.VenueId == v.VenueId && img.IsPrimary).ImageUrl
                           ?? "/Content/images/bg_landingpage.jpg"
            }).ToList();

            ViewBag.VenueSelectList = new SelectList(db.Venues.Where(v => v.Status == 1).Select(v => new { v.VenueId, v.Name }), "VenueId", "Name");

            // Dịch vụ đi kèm đang hoạt động, cho Index.cshtml (thay cho việc gọi db trực tiếp trong View)
            ViewBag.HomeAddOns = db.AddOnServices
                .Where(a => a.IsActive)
                .OrderBy(a => a.Category)
                .ThenBy(a => a.Name)
                .ToList();

            // Lưu lại để giữ trạng thái trên View
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = string.IsNullOrEmpty(category) ? "All" : category;
            ViewBag.CurrentDate = eventDate;
            ViewBag.CurrentMaxCapacity = maxCapacity;
            ViewBag.CurrentMaxPrice = maxPrice;

            return View(venues);
        }

        // Trả lịch đặt cho Calendar ("XEM TẤT CẢ SẢNH"; chọn 1 sảnh thì truyền venueId)
        [HttpGet]
        public JsonResult GetAllBookings(int? venueId)
        {
            var bookingQuery = db.Bookings.Where(b => b.Status != 4);   // 4 = đã huỷ

            if (venueId.HasValue)
            {
                bookingQuery = bookingQuery.Where(b => b.VenueId == venueId.Value);
            }

            // Lấy dữ liệu ra trước rồi mới format ngày (ToString không dịch được sang SQL)
            var raw = bookingQuery
                .Select(b => new
                {
                    b.BookingId,
                    VenueName = b.Venue.Name,
                    b.EventStartDateTime,
                    b.EventEndDateTime
                })
                .ToList();

            // Chỉ trả thông tin tối thiểu, KHÔNG trả tên/SĐT khách vì trang này công khai
            var events = raw.Select(b => new
            {
                id = b.BookingId,
                title = b.VenueName + " (Đã đặt)",
                start = b.EventStartDateTime.ToString("s"),
                end = b.EventEndDateTime.ToString("s")
            });

            return Json(events, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}