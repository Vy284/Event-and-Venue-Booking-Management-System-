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
    public class VenueImagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET
        public ActionResult Index()
        {
            var venueImages = db.VenueImages.Include(v => v.Venue);
            return View(venueImages.ToList());
        }

        // GET: Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VenueImage venueImage = db.VenueImages.Find(id);
            if (venueImage == null)
            {
                return HttpNotFound();
            }
            return View(venueImage);
        }

        // GET: Create
        public ActionResult Create()
        {
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name");
            return View();
        }

        // POST: Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ImageId,VenueId,ImageUrl,IsPrimary")] VenueImage venueImage)
        {
            if (ModelState.IsValid)
            {
                if (venueImage.IsPrimary)
                {
                    bool hasPrimary = db.VenueImages
                        .Any(x => x.VenueId == venueImage.VenueId && x.IsPrimary);

                    if (hasPrimary)
                    {
                        ModelState.AddModelError(
                            "IsPrimary",
                            "Địa điểm này đã có ảnh chính. Vui lòng chỉnh sửa ảnh chính hiện tại nếu muốn thay đổi."
                        );
                    }
                }
                if (ModelState.IsValid)
                {
                    db.VenueImages.Add(venueImage);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name", venueImage.VenueId);
            return View(venueImage);
        }

        // GET: Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VenueImage venueImage = db.VenueImages.Find(id);
            if (venueImage == null)
            {
                return HttpNotFound();
            }
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name", venueImage.VenueId);
            return View(venueImage);
        }

        // POST: Edit
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ImageId,VenueId,ImageUrl,IsPrimary")] VenueImage venueImage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(venueImage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name", venueImage.VenueId);
            return View(venueImage);
        }

        // GET: VenueImages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VenueImage venueImage = db.VenueImages.Find(id);
            if (venueImage == null)
            {
                return HttpNotFound();
            }
            return View(venueImage);
        }

        // POST: VenueImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            VenueImage venueImage = db.VenueImages.Find(id);
            if (venueImage == null)
                return HttpNotFound();
            if (venueImage.IsPrimary)
            {
                venueImage.IsPrimary = false;
            }
            else
            {
                db.VenueImages.Remove(venueImage);
            }
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
