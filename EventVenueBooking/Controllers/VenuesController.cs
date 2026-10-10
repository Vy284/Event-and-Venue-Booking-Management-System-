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
using EventVenueBooking.Filters;
using EventVenueBooking.Services;                   // để dùng BookingStatuses, VenueStatuses
using EventVenueBooking.ViewModels;

namespace EventVenueBooking.Controllers
{
    // SỬA: Coordinator được XEM (Index, Details, Facilities_Services). Thêm/sửa/xóa chỉ Admin.
    [CustomAuthorize(Roles = "Admin,Coordinator")]
    public class VenuesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Lấy UserId người đang đăng nhập (ticket lưu Name = Email)
        private int CurrentUserId()
        {
            string email = User.Identity.Name;
            return db.Users.Where(u => u.Email == email).Select(u => u.UserId).First();
        }

        // Sảnh còn booking chưa kết thúc (chưa hủy, chưa hoàn tất) hay không
        private bool HasUpcomingBookings(int venueId)
        {
            return db.Bookings.Any(b =>
                b.VenueId == venueId &&
                b.Status != BookingStatuses.Cancelled &&
                b.Status != BookingStatuses.Completed &&
                b.EventEndDateTime > DateTime.Now);
        }

        // GET: Venues
        public ActionResult Index()
        {
            var venues = db.Venues.Select(v => new VenueListViewModel
            {
                VenueId = v.VenueId,
                VenueName = v.Name,
                Capacity = v.Capacity,
                PricePerHour = v.RentalRate,
                RentalUnit = v.RentalUnit,
                TypeName = v.VenueType != null ? v.VenueType.TypeName : "N/A",

                // Lấy hình ảnh chính. Ảnh dự phòng thống nhất với trang chủ
                PrimaryImageUrl = v.VenueImages.FirstOrDefault(img => img.IsPrimary).ImageUrl ?? "/Content/images/bg_landingpage.jpg",

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
            // Theo Venue.cs: 0 = Inactive, 1 = Active, 2 = UnderMaintenance
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
        [CustomAuthorize(Roles = "Admin")]              // SỬA: chỉ Admin
        public ActionResult Create()
        {
            ViewBag.CreatedByUserId = new SelectList(db.Users, "UserId", "FullName");
            ViewBag.VenueTypeId = new SelectList(db.VenueTypes, "VenueTypeId", "TypeName");
            return View();
        }

        // POST: Create
        // Bỏ CreatedAt và CreatedByUserId khỏi Bind, server tự điền từ người đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]              // SỬA: chỉ Admin
        public ActionResult Create([Bind(Include = "VenueId,Name,VenueTypeId,Capacity,Description,RentalRate,RentalUnit,Location,Status")] Venue venue)
        {
            venue.CreatedByUserId = CurrentUserId();
            venue.CreatedAt = DateTime.Now;
            ModelState.Remove("CreatedByUserId");       // giá trị đã được server điền
            ModelState.Remove("CreatedAt");

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
        [CustomAuthorize(Roles = "Admin")]              // SỬA: chỉ Admin
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
        // Không bind CreatedAt / CreatedByUserId; nạp bản gốc từ DB rồi chỉ cập nhật các trường được phép sửa.
        // Không cho đổi sảnh khỏi trạng thái Active khi còn booking chưa kết thúc.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]              // SỬA: chỉ Admin
        public ActionResult Edit([Bind(Include = "VenueId,Name,VenueTypeId,Capacity,Description,RentalRate,RentalUnit,Location,Status")] Venue venue)
        {
            ModelState.Remove("CreatedByUserId");       // không lấy từ form
            ModelState.Remove("CreatedAt");

            if (ModelState.IsValid)
            {
                var existing = db.Venues.Find(venue.VenueId);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                if (existing.Status == VenueStatuses.Active
                    && venue.Status != VenueStatuses.Active
                    && HasUpcomingBookings(existing.VenueId))
                {
                    ModelState.AddModelError("Status",
                        "Sảnh còn booking chưa hoàn tất, hãy xử lý các booking đó trước khi ngừng hoạt động hoặc bảo trì.");
                }
                else
                {
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
            }

            ViewBag.CreatedByUserId = new SelectList(db.Users, "UserId", "FullName", venue.CreatedByUserId);
            ViewBag.VenueTypeId = new SelectList(db.VenueTypes, "VenueTypeId", "TypeName", venue.VenueTypeId);
            return View(venue);
        }

        // GET: Delete
        [CustomAuthorize(Roles = "Admin")]              // SỬA: chỉ Admin
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
        // Chặn khi sảnh còn booking chưa kết thúc
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]              // SỬA: chỉ Admin
        public ActionResult DeleteConfirmed(int id)
        {
            Venue venue = db.Venues.Find(id);
            if (venue == null)
            {
                return HttpNotFound();
            }

            if (HasUpcomingBookings(id))
            {
                TempData["ErrorMessage"] = "Sảnh còn booking chưa hoàn tất, hãy xử lý các booking đó trước khi ngừng hoạt động.";
                return RedirectToAction("Index");
            }

            venue.Status = VenueStatuses.Inactive;
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