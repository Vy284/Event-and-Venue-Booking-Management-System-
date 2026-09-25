using EventVenueBooking.Database;
using EventVenueBooking.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EventVenueBooking.Controllers
{
    public class VenueTypesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: VenueTypes
        public ActionResult Index()
        {
            List<VenueType> venueTypes = db.VenueTypes.ToList();
            return View(venueTypes);
        }

        // GET: VenueTypes/Create
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VenueType venueType) 
        {
            if (ModelState.IsValid)
            {
                db.VenueTypes.Add(venueType);
                db.SaveChanges();
                return RedirectToAction("Index");
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VenueType venueType)
        {
            if (ModelState.IsValid) 
            {
                db.Entry(venueType).State = EntityState.Modified;
                db.SaveChanges();
                //return Content("Đã nhận: " + venueType.TypeName);
                return RedirectToAction("Index");
            }
            return View(venueType);
        }



    }
}