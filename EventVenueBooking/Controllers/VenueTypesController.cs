using EventVenueBooking.Database;
using EventVenueBooking.Entities;
using EventVenueBooking.Filters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EventVenueBooking.Controllers
{
    [CustomAuthorize(Roles = "Admin")]
    public class VenueTypesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET
        public ActionResult Index()
        {
            List<VenueType> venueTypes = db.VenueTypes.ToList();
            return View(venueTypes);
        }

        // Detail
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VenueType venueType = db.VenueTypes.Find(id);
            if (venueType == null)
            {
                return HttpNotFound();
            }

            return View(venueType);
        }

        // GET: VenueTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Create
        // SỬA: thêm [Bind], kiểm tra trùng tên (TypeName có unique index)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TypeName,IsActive")] VenueType venueType)
        {
            if (ModelState.IsValid)
            {
                venueType.TypeName = venueType.TypeName.Trim();

                if (db.VenueTypes.Any(v => v.TypeName == venueType.TypeName))
                {
                    ModelState.AddModelError("TypeName", "Tên loại địa điểm này đã tồn tại.");
                }
                else
                {
                    db.VenueTypes.Add(venueType);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(venueType);
        }

        //Get/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            VenueType venueType = db.VenueTypes.Find(id);
            if (venueType == null)
            {
                return HttpNotFound();
            }

            return View(venueType);
        }

        // POST: Edit
        // SỬA: thêm [Bind], nạp bản gốc từ DB rồi chỉ cập nhật các trường được phép sửa, kiểm tra trùng tên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "VenueTypeId,TypeName,IsActive")] VenueType venueType)
        {
            if (ModelState.IsValid)
            {
                var existing = db.VenueTypes.Find(venueType.VenueTypeId);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                string newName = venueType.TypeName.Trim();

                if (db.VenueTypes.Any(v => v.TypeName == newName && v.VenueTypeId != venueType.VenueTypeId))
                {
                    ModelState.AddModelError("TypeName", "Tên loại địa điểm này đã tồn tại.");
                }
                else
                {
                    existing.TypeName = newName;
                    existing.IsActive = venueType.IsActive;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(venueType);
        }

        //delete
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            VenueType venueType = db.VenueTypes.Find(id);
            if (venueType == null) return HttpNotFound();
            return View(venueType);
        }

        // POST: Delete (không xóa thật, chỉ tắt IsActive)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int id)
        {
            VenueType venueType = db.VenueTypes.Find(id);
            if (venueType == null)
            {
                return HttpNotFound();
            }
            venueType.IsActive = false;
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