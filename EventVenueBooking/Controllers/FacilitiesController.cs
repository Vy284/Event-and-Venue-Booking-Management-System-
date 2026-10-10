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

namespace EventVenueBooking.Controllers
{
    // SỬA: Coordinator được XEM (Index, Details). Thêm/sửa/xóa chỉ Admin.
    [CustomAuthorize(Roles = "Admin,Coordinator")]
    public class FacilitiesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Facilities
        public ActionResult Index()
        {
            return View(db.Facilities.ToList());
        }

        // GET: Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Facility facility = db.Facilities.Find(id);
            if (facility == null)
            {
                return HttpNotFound();
            }
            return View(facility);
        }

        // GET: Create
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult Create([Bind(Include = "FacilityId,FacilityName,IsActive")] Facility facility)
        {
            if (ModelState.IsValid)
            {
                db.Facilities.Add(facility);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(facility);
        }

        // GET: Edit
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Facility facility = db.Facilities.Find(id);
            if (facility == null)
            {
                return HttpNotFound();
            }
            return View(facility);
        }

        // POST: Edit
        // Có [Bind], nạp bản gốc từ DB rồi chỉ cập nhật các trường được phép sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult Edit([Bind(Include = "FacilityId,FacilityName,IsActive")] Facility facility)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Facilities.Find(facility.FacilityId);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.FacilityName = facility.FacilityName;
                existing.IsActive = facility.IsActive;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(facility);
        }

        // GET: Delete
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Facility facility = db.Facilities.Find(id);
            if (facility == null)
            {
                return HttpNotFound();
            }
            return View(facility);
        }

        // POST: Delete (không xóa thật, chỉ tắt IsActive)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult DeleteConfirmed(int id)
        {
            Facility facility = db.Facilities.Find(id);
            if (facility == null) return HttpNotFound();
            facility.IsActive = false;
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