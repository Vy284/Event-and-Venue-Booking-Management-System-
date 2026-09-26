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

namespace EventVenueBooking.Controllers
{
    public class VenuesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Venues
        public ActionResult Index()
        {
            var venues = db.Venues.Include(v => v.CreatedByUser).Include(v => v.VenueType);
            return View(venues.ToList());
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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
            venue.Status = 0;
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
