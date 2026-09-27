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
    public class BookingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Bookings
        public ActionResult Index()
        {
            var bookings = db.Bookings.Include(b => b.CancelledByUser).Include(b => b.ClientUser).Include(b => b.EventType).Include(b => b.LastModifiedByUser).Include(b => b.Venue);
            return View(bookings.ToList());
        }

        // GET: Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // GET: Create
        public ActionResult Create()
        {
            ViewBag.CancelledByUserId = new SelectList(db.Users, "UserId", "FullName");
            ViewBag.ClientUserId = new SelectList(db.Users, "UserId", "FullName");
            ViewBag.EventTypeId = new SelectList(db.EventTypes, "EventTypeId", "TypeName");
            ViewBag.LastModifiedByUserId = new SelectList(db.Users, "UserId", "FullName");
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name");
            return View();
        }

        // POST: Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookingId,ClientUserId,VenueId,EventTypeId,GuestCount,EventStartDateTime,EventEndDateTime,Status,VenueRateAtBooking,RentalUnitAtBooking,RentalQuantity,VenueCost,AddOnCost,TotalCost,CreatedAt,LastModifiedByUserId,LastModifiedAt,CancelledByUserId,CancelledAt,CancelReason")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Bookings.Add(booking);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CancelledByUserId = new SelectList(db.Users, "UserId", "FullName", booking.CancelledByUserId);
            ViewBag.ClientUserId = new SelectList(db.Users, "UserId", "FullName", booking.ClientUserId);
            ViewBag.EventTypeId = new SelectList(db.EventTypes, "EventTypeId", "TypeName", booking.EventTypeId);
            ViewBag.LastModifiedByUserId = new SelectList(db.Users, "UserId", "FullName", booking.LastModifiedByUserId);
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name", booking.VenueId);
            return View(booking);
        }

        // GET: Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            ViewBag.CancelledByUserId = new SelectList(db.Users, "UserId", "FullName", booking.CancelledByUserId);
            ViewBag.ClientUserId = new SelectList(db.Users, "UserId", "FullName", booking.ClientUserId);
            ViewBag.EventTypeId = new SelectList(db.EventTypes, "EventTypeId", "TypeName", booking.EventTypeId);
            ViewBag.LastModifiedByUserId = new SelectList(db.Users, "UserId", "FullName", booking.LastModifiedByUserId);
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name", booking.VenueId);
            return View(booking);
        }

        // POST: Edit
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookingId,ClientUserId,VenueId,EventTypeId,GuestCount,EventStartDateTime,EventEndDateTime,Status,VenueRateAtBooking,RentalUnitAtBooking,RentalQuantity,VenueCost,AddOnCost,TotalCost,CreatedAt,LastModifiedByUserId,LastModifiedAt,CancelledByUserId,CancelledAt,CancelReason")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(booking).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CancelledByUserId = new SelectList(db.Users, "UserId", "FullName", booking.CancelledByUserId);
            ViewBag.ClientUserId = new SelectList(db.Users, "UserId", "FullName", booking.ClientUserId);
            ViewBag.EventTypeId = new SelectList(db.EventTypes, "EventTypeId", "TypeName", booking.EventTypeId);
            ViewBag.LastModifiedByUserId = new SelectList(db.Users, "UserId", "FullName", booking.LastModifiedByUserId);
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name", booking.VenueId);
            return View(booking);
        }

        // GET: Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            //db.Bookings.Remove(booking);
            booking.Status = 4; // Cancelled
            booking.CancelledAt = DateTime.Now;
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
