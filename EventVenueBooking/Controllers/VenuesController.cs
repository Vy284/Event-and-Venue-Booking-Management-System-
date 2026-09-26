using EventVenueBooking.Database;
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace EventVenueBooking.Controllers
{
    public class VenuesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Venues
        public ActionResult Index()
        {
            // Query trực tiếp từ DB sang ViewModel bằng LINQ Select
            var venues = db.Venues.Select(v => new VenueListViewModel
            {
                VenueId = v.VenueId,
                VenueName = v.Name,
                Capacity = v.Capacity,
                PricePerHour = v.RentalRate,
                RentalUnit = v.RentalUnit,
                TypeName = v.VenueType != null ? v.VenueType.TypeName : "N/A",

                // Lấy hình ảnh chính
                PrimaryImageUrl = v.VenueImages.FirstOrDefault(img => img.IsPrimary).ImageUrl ?? "/images/default-venue.jpg",

                Status = v.Status,
                Location = v.Location, // Hoặc v.Address tuỳ tên thuộc tính trong Entity Venue của m
                Description = v.Description,

                // 1. TÍNH RATING TRUNG BÌNH TỪ BẢNG FEEDBACK
                // (Nếu chưa có feedback nào thì mặc định là 0.0)
                Rating = db.Feedbacks
                    .Where(f => f.Booking.VenueId == v.VenueId)
                    .Select(f => (double?)f.Rating)
                    .Average() ?? 0.0,

                // 2. LẤY NGÀY ĐẶT GẦN NHẤT TỪ BẢNG BOOKING
                LastBooking = db.Bookings
                    .Where(b => b.VenueId == v.VenueId)
                    .OrderByDescending(b => b.EventEndDateTime)
                    .Select(b => (DateTime?)b.EventEndDateTime)
                    .FirstOrDefault()

            }).ToList();

            // Thống kê số lượng cho 4 card trên UI
            ViewBag.TotalVenues = venues.Count;
            ViewBag.ActiveVenues = venues.Count(v => v.Status == 0);
            ViewBag.InactiveVenues = venues.Count(v => v.Status == 1);
            ViewBag.MaintenanceVenues = venues.Count(v => v.Status == 2);

            return View("~/Views/Admin/Venue.cshtml", venues);
        }

        public ActionResult Facilities_Services()
        {
            var model = new FacilitiesServicesViewModel
            {
                Facilities = db.Facilities.Select(f => new FacilityViewModel
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    IsActive = f.IsActive,
                    LinkedVenuesCount = f.Venues.Count // Đếm số venue liên kết từ bảng nối[cite: 53]
                }).ToList(),

                Services = db.AddOnServices.Select(s => new AddOnServiceViewModel
                {
                    AddOnId = s.AddOnId,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    Category = s.Category,
                    IsActive = s.IsActive
                }).ToList()
            };

            // Thống kê cho 4 card Viewbag
            ViewBag.TotalFacilities = model.Facilities.Count;
            ViewBag.ActiveFacilities = model.Facilities.Count(f => f.IsActive);
            ViewBag.TotalServices = model.Services.Count;
            ViewBag.ActiveServices = model.Services.Count(s => s.IsActive);

            return View("~/Views/Admin/Facilities_Services.cshtml", model);
        }
    }
}