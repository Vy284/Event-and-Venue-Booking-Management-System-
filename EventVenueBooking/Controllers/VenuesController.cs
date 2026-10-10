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
using EventVenueBooking.Services;
using EventVenueBooking.ViewModels;

namespace EventVenueBooking.Controllers
{
    // Coordinator được XEM (Index, Details, Facilities_Services). Thêm/sửa/xóa chỉ Admin.
    [CustomAuthorize(Roles = "Admin,Coordinator")]
    public class VenuesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private int CurrentUserId()
        {
            string email = User.Identity.Name;
            return db.Users.Where(u => u.Email == email).Select(u => u.UserId).First();
        }

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
                PrimaryImageUrl = v.VenueImages.FirstOrDefault(img => img.IsPrimary).ImageUrl
                                  ?? "/Content/images/bg_landingpage.jpg",
                Status = v.Status,
                Location = v.Location,
                Description = v.Description,
                Rating = db.Feedbacks
                    .Where(f => f.Booking.VenueId == v.VenueId)
                    .Select(f => (double?)f.Rating)
                    .Average() ?? 0.0,
                LastBooking = db.Bookings
                    .Where(b => b.VenueId == v.VenueId)
                    .OrderByDescending(b => b.EventEndDateTime)
                    .Select(b => (DateTime?)b.EventEndDateTime)
                    .FirstOrDefault()
            }).ToList();

            ViewBag.TotalVenues = venues.Count;
            ViewBag.ActiveVenues = venues.Count(v => v.Status == 1);
            ViewBag.InactiveVenues = venues.Count(v => v.Status == 0);
            ViewBag.MaintenanceVenues = venues.Count(v => v.Status == 2);

            // Dropdown loại sảnh (chỉ active)
            ViewBag.VenueTypes = db.VenueTypes
                .Where(t => t.IsActive)
                .OrderBy(t => t.TypeName)
                .Select(t => new SelectListItem
                {
                    Value = t.VenueTypeId.ToString(),
                    Text = t.TypeName
                })
                .ToList();

            // Checkbox facility (chỉ active)
            ViewBag.Facilities = db.Facilities
                .Where(f => f.IsActive)
                .OrderBy(f => f.FacilityName)
                .Select(f => new SelectListItem
                {
                    Value = f.FacilityId.ToString(),
                    Text = f.FacilityName
                })
                .ToList();

            // Map VenueId -> list FacilityId (tick sẵn khi Edit)
            ViewBag.VenueFacilityMap = db.VenueFacilities
                .GroupBy(vf => vf.VenueId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.FacilityId).ToList());

            // Map VenueId -> VenueTypeId (chọn đúng type khi Edit)
            ViewBag.VenueTypeMap = db.Venues
                .ToDictionary(v => v.VenueId, v => v.VenueTypeId);

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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Venue venue = db.Venues.Find(id);
            if (venue == null)
                return HttpNotFound();

            return View(venue);
        }

        // GET: Create (scaffold — form chính dùng modal trên Index)
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.CreatedByUserId = new SelectList(db.Users, "UserId", "FullName");
            ViewBag.VenueTypeId = new SelectList(db.VenueTypes.Where(t => t.IsActive), "VenueTypeId", "TypeName");
            return View();
        }

        // POST: Create — nhận thêm facilityIds từ checkbox
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Create(
            [Bind(Include = "VenueId,Name,VenueTypeId,Capacity,Description,RentalRate,RentalUnit,Location,Status")] Venue venue,
            int[] facilityIds)
        {
            venue.CreatedByUserId = CurrentUserId();
            venue.CreatedAt = DateTime.Now;
            ModelState.Remove("CreatedByUserId");
            ModelState.Remove("CreatedAt");

            if (ModelState.IsValid)
            {
                db.Venues.Add(venue);
                db.SaveChanges();

                if (facilityIds != null)
                {
                    foreach (var fid in facilityIds.Distinct())
                    {
                        db.VenueFacilities.Add(new VenueFacility
                        {
                            VenueId = venue.VenueId,
                            FacilityId = fid
                        });
                    }
                    db.SaveChanges();
                }

                TempData["SuccessMessage"] = "Đã tạo sảnh thành công.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Không tạo được sảnh. Kiểm tra lại form.";
            return RedirectToAction("Index");
        }

        // GET: Edit (scaffold)
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Venue venue = db.Venues.Find(id);
            if (venue == null)
                return HttpNotFound();

            ViewBag.CreatedByUserId = new SelectList(db.Users, "UserId", "FullName", venue.CreatedByUserId);
            ViewBag.VenueTypeId = new SelectList(db.VenueTypes.Where(t => t.IsActive), "VenueTypeId", "TypeName", venue.VenueTypeId);
            return View(venue);
        }

        // POST: Edit — nhận thêm facilityIds
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Edit(
            [Bind(Include = "VenueId,Name,VenueTypeId,Capacity,Description,RentalRate,RentalUnit,Location,Status")] Venue venue,
            int[] facilityIds)
        {
            ModelState.Remove("CreatedByUserId");
            ModelState.Remove("CreatedAt");

            if (ModelState.IsValid)
            {
                var existing = db.Venues.Find(venue.VenueId);
                if (existing == null)
                    return HttpNotFound();

                if (existing.Status == VenueStatuses.Active
                    && venue.Status != VenueStatuses.Active
                    && HasUpcomingBookings(existing.VenueId))
                {
                    TempData["ErrorMessage"] = "Sảnh còn booking chưa hoàn tất, không thể đổi trạng thái.";
                    return RedirectToAction("Index");
                }

                existing.Name = venue.Name;
                existing.VenueTypeId = venue.VenueTypeId;
                existing.Capacity = venue.Capacity;
                existing.Description = venue.Description;
                existing.RentalRate = venue.RentalRate;
                existing.RentalUnit = venue.RentalUnit;
                existing.Location = venue.Location;
                existing.Status = venue.Status;

                // Cập nhật facilities: xóa cũ, thêm mới
                var oldLinks = db.VenueFacilities.Where(vf => vf.VenueId == existing.VenueId).ToList();
                db.VenueFacilities.RemoveRange(oldLinks);

                if (facilityIds != null)
                {
                    foreach (var fid in facilityIds.Distinct())
                    {
                        db.VenueFacilities.Add(new VenueFacility
                        {
                            VenueId = existing.VenueId,
                            FacilityId = fid
                        });
                    }
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã cập nhật sảnh.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Không cập nhật được sảnh.";
            return RedirectToAction("Index");
        }

        // GET: Delete
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Venue venue = db.Venues.Find(id);
            if (venue == null)
                return HttpNotFound();

            return View(venue);
        }

        // POST: Delete (chỉ chuyển Inactive)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            Venue venue = db.Venues.Find(id);
            if (venue == null)
                return HttpNotFound();

            if (HasUpcomingBookings(id))
            {
                TempData["ErrorMessage"] = "Sảnh còn booking chưa hoàn tất, hãy xử lý các booking đó trước khi ngừng hoạt động.";
                return RedirectToAction("Index");
            }

            venue.Status = VenueStatuses.Inactive;
            db.SaveChanges();
            TempData["SuccessMessage"] = "Đã ngừng hoạt động sảnh.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}