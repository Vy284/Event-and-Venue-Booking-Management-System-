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
        public ActionResult Create()
        {
            return View();
        }

        // POST: Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AddOnService addOnService = db.AddOnServices.Find(id);
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
