using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EventVenueBooking.Database;
using EventVenueBooking.Entities;
using EventVenueBooking.Filters;                    // THÊM
using EventVenueBooking.ViewModels;

namespace EventVenueBooking.Controllers
{
    [CustomAuthorize(Roles = "Admin")]              // THÊM
    public class VenuesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // THÊM: lấy UserId người đang đăng nhập (ticket lưu Name = Email)
        private int CurrentUserId()
        {
            string email = User.Identity.Name;
            return db.Users.Where(u => u.Email == email).Select(u => u.UserId).First();
        }

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
                Location = v.Location,
                Description = v.Description,

                // Rating trung bình từ bảng Feedback (chưa có thì là 0.0)
                Rating = db.Feedbacks
                    .Where(f => f.Booking.VenueId == v.VenueId)
                    .Select(f => (double?)f.Rating)
                    .Average() ?? 0.0,

                // Ngày đặt gần nhất từ bảng Booking
                LastBooking = db.Bookings
                    .Where(b => b.VenueId == v.VenueId)
                    .OrderByDescending(b => b.EventEndDateTime)
                    .Select(b => (DateTime?)b.EventEndDateTime)
                    .FirstOrDefault()

            }).ToList();

            // Thống kê số lượng cho 4 card trên UI
            // SỬA: theo Venue.cs: 0 = Inactive, 1 = Active, 2 = UnderMaintenance
            // (bản cũ đang đếm ngược: Active == 0, Inactive == 1)
            ViewBag.TotalVenues = venues.Count;
            ViewBag.ActiveVenues = venues.Count(v => v.Status == 1);
            ViewBag.InactiveVenues = venues.Count(v => v.Status == 0);
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
                    LinkedVenuesCount = f.VenueFacilities.Count
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

            ViewBag.TotalFacilities = model.Facilities.Count;
            ViewBag.ActiveFacilities = model.Facilities.Count(f => f.IsActive);
            ViewBag.TotalServices = model.Services.Count;
            ViewBag.ActiveServices = model.Services.Count(s => s.IsActive);

            return View("~/Views/Admin/Facilities_Services.cshtml", model);
        }

        // GET: Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venue venue = db.Venues.Find(id);
            if (venue == null)
            {
                return HttpNotFound();
            }
            return View(venue);
        }

        // GET: Create
        public ActionResult Create()
        {
            ViewBag.CreatedByUserId = new SelectList(db.Users, "UserId", "FullName");
            ViewBag.VenueTypeId = new SelectList(db.VenueTypes, "VenueTypeId", "TypeName");
            return View();
        }

        // POST: Create
        // SỬA: bỏ CreatedAt và CreatedByUserId khỏi Bind, server tự điền từ người đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "VenueId,Name,VenueTypeId,Capacity,Description,RentalRate,RentalUnit,Location,Status")] Venue venue)
        {
            venue.CreatedByUserId = CurrentUserId();    // THÊM
            venue.CreatedAt = DateTime.Now;             // THÊM
            ModelState.Remove("CreatedByUserId");       // THÊM: giá trị đã được server điền
            ModelState.Remove("CreatedAt");             // THÊM

            if (ModelState.IsValid)
            {
                db.Venues.Add(venue);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedByUserId = new SelectList(db.Users, "UserId", "FullName", venue.CreatedByUserId);
            ViewBag.VenueTypeId = new SelectList(db.VenueTypes, "VenueTypeId", "TypeName", venue.VenueTypeId);
            return View(venue);
        }

        // GET: Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venue venue = db.Venues.Find(id);
            if (venue == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedByUserId = new SelectList(db.Users, "UserId", "FullName", venue.CreatedByUserId);
            ViewBag.VenueTypeId = new SelectList(db.VenueTypes, "VenueTypeId", "TypeName", venue.VenueTypeId);
            return View(venue);
        }

        // POST: Edit
        // SỬA: không bind CreatedAt / CreatedByUserId; nạp bản gốc từ DB rồi chỉ cập nhật các trường được phép sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "VenueId,Name,VenueTypeId,Capacity,Description,RentalRate,RentalUnit,Location,Status")] Venue venue)
        {
            ModelState.Remove("CreatedByUserId");       // THÊM: không lấy từ form
            ModelState.Remove("CreatedAt");

            if (ModelState.IsValid)
            {
                var existing = db.Venues.Find(venue.VenueId);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.Name = venue.Name;
                existing.VenueTypeId = venue.VenueTypeId;
                existing.Capacity = venue.Capacity;
                existing.Description = venue.Description;
                existing.RentalRate = venue.RentalRate;
                existing.RentalUnit = venue.RentalUnit;
                existing.Location = venue.Location;
                existing.Status = venue.Status;
                // CreatedAt và CreatedByUserId giữ nguyên

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedByUserId = new SelectList(db.Users, "UserId", "FullName", venue.CreatedByUserId);
            ViewBag.VenueTypeId = new SelectList(db.VenueTypes, "VenueTypeId", "TypeName", venue.VenueTypeId);
            return View(venue);
        }

        // GET: Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venue venue = db.Venues.Find(id);
            if (venue == null)
            {
                return HttpNotFound();
            }
            return View(venue);
        }

        // POST: Delete (không xóa thật, chỉ chuyển sang Inactive)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Venue venue = db.Venues.Find(id);
            if (venue == null)                      // THÊM: tránh NullReference
            {
                return HttpNotFound();
            }
            venue.Status = 0; // 0 = Inactive (đúng theo Venue.cs)
            db.SaveChanges();
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