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
    public class AddOnServicesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AddOnServices
        public ActionResult Index()
        {
            return View(db.AddOnServices.ToList());
        }

        // GET: Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AddOnService addOnService = db.AddOnServices.Find(id);
            if (addOnService == null)
            {
                return HttpNotFound();
            }
            return View(addOnService);
        }

        // GET: /Create
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult Create([Bind(Include = "AddOnId,Name,Description,Price,Category,IsActive")] AddOnService addOnService)
        {
            if (ModelState.IsValid)
            {
                db.AddOnServices.Add(addOnService);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(addOnService);
        }

        // GET: Edit
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AddOnService addOnService = db.AddOnServices.Find(id);
            if (addOnService == null)
            {
                return HttpNotFound();
            }
            return View(addOnService);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult Edit([Bind(Include = "AddOnId,Name,Description,Price,Category,IsActive")] AddOnService addOnService)
        {
            if (ModelState.IsValid)
            {
                db.Entry(addOnService).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(addOnService);
        }

        // GET: Delete
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AddOnService addOnService = db.AddOnServices.Find(id);
            if (addOnService == null)
            {
                return HttpNotFound();
            }
            return View(addOnService);
        }

        // POST: Delete (không xóa thật, chỉ tắt IsActive)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult DeleteConfirmed(int id)
        {
            AddOnService addOnService = db.AddOnServices.Find(id);
            if (addOnService == null)
            {
                return HttpNotFound();
            }
            addOnService.IsActive = false;
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