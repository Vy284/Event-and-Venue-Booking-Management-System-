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
using EventVenueBooking.ViewModels;

namespace EventVenueBooking.Controllers
{
    public class VenuesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

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
        public ActionResult Create() //nghiệp vụ autofill CreatedByUserId => User + Authentication
        {
            ViewBag.CreatedByUserId = new SelectList(db.Users, "UserId", "FullName");
            ViewBag.VenueTypeId = new SelectList(db.VenueTypes, "VenueTypeId", "TypeName");
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken] //field không nên cho người dùng tự gửi lên: CreatedAt, CreatedByUserId => Authentication
        public ActionResult Create([Bind(Include = "VenueId,Name,VenueTypeId,Capacity,Description,RentalRate,RentalUnit,Location,Status,CreatedAt,CreatedByUserId")] Venue venue)
        {
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
        public ActionResult Edit(int? id) //CreatedAt và CreatedByUserId cần được bảo vệ.
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "VenueId,Name,VenueTypeId,Capacity,Description,RentalRate,RentalUnit,Location,Status,CreatedAt,CreatedByUserId")] Venue venue)
        {
            if (ModelState.IsValid)
            {
                db.Entry(venue).State = EntityState.Modified;
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

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Venue venue = db.Venues.Find(id);
            venue.Status = 0; // TODO: kiểm tra lại giá trị Status, xem chú ý bên dưới
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