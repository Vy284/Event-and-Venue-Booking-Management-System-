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
    public class EventTypesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EventTypes
        public ActionResult Index()
        {
            return View(db.EventTypes.ToList());
        }

        // GET: Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EventType eventType = db.EventTypes.Find(id);
            if (eventType == null)
            {
                return HttpNotFound();
            }
            return View(eventType);
        }

        // GET: Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EventType eventType)
        {
            if (ModelState.IsValid)
            {
                db.EventTypes.Add(eventType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(eventType);
        }

        // GET: Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EventType eventType = db.EventTypes.Find(id);
            if (eventType == null)
            {
                return HttpNotFound();
            }
            return View(eventType);
        }

        // POST: Edit
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EventType eventType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(eventType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(eventType);
        }

        // GET: EventTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EventType eventType = db.EventTypes.Find(id);
            if (eventType == null)
            {
                return HttpNotFound();
            }
            return View(eventType);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EventType eventType = db.EventTypes.Find(id);
            eventType.IsActive = false;
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
