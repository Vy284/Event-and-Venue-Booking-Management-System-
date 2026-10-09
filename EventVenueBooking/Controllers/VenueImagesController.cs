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
    [CustomAuthorize(Roles = "Admin")]
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
        // SỬA: nạp bản gốc từ DB rồi chỉ cập nhật các trường được phép sửa (có kiểm tra tồn tại)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ImageId,VenueId,ImageUrl,IsPrimary")] VenueImage venueImage)
        {
            if (ModelState.IsValid)
            {
                var existing = db.VenueImages.Find(venueImage.ImageId);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                if (venueImage.IsPrimary)
                {
                    // Ảnh này thành ảnh chính thì bỏ cờ ảnh chính của các ảnh khác cùng sảnh
                    var oldPrimaryImages = db.VenueImages
                        .Where(x => x.VenueId == venueImage.VenueId
                                 && x.IsPrimary
                                 && x.ImageId != venueImage.ImageId)
                        .ToList();

                    foreach (var image in oldPrimaryImages)
                    {
                        image.IsPrimary = false;
                    }
                }

                existing.VenueId = venueImage.VenueId;
                existing.ImageUrl = venueImage.ImageUrl;
                existing.IsPrimary = venueImage.IsPrimary;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name", venueImage.VenueId);
            return View(venueImage);
        }

        // GET: Delete
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

        // POST: Delete
        // SỬA: ảnh chính không bị xóa ngầm nữa; phải chọn ảnh khác làm ảnh chính trước
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            VenueImage venueImage = db.VenueImages.Find(id);
            if (venueImage == null)
                return HttpNotFound();

            if (venueImage.IsPrimary)
            {
                TempData["ErrorMessage"] = "Đây là ảnh chính của địa điểm. Hãy chọn ảnh khác làm ảnh chính trước khi xóa ảnh này.";
                return RedirectToAction("Index");
            }

            db.VenueImages.Remove(venueImage);
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