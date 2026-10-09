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
        // SỬA: thêm [Bind] chỉ nhận các trường được phép nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TypeName,IsActive")] EventType eventType)
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
        // SỬA: thêm [Bind], nạp bản gốc từ DB rồi chỉ cập nhật các trường được phép sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EventTypeId,TypeName,IsActive")] EventType eventType)
        {
            if (ModelState.IsValid)
            {
                var existing = db.EventTypes.Find(eventType.EventTypeId);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.TypeName = eventType.TypeName;
                existing.IsActive = eventType.IsActive;

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

        // POST: Delete (không xóa thật, chỉ tắt IsActive)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EventType eventType = db.EventTypes.Find(id);
            if (eventType == null)
            {
                return HttpNotFound();
            }
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