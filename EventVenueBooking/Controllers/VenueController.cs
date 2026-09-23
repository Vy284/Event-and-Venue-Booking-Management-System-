using EventVenueBooking.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace EventVenueBooking.Controllers
{
    public class VenueController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Venue/Details/1
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var venue = db.Venues
                .Include(v => v.VenueType)
                .Include(v => v.Facilities)
                .FirstOrDefault(v => v.VenueId == id);

            if (venue == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách Dịch vụ đi kèm (Add-ons) và Ảnh của sảnh
            ViewBag.Images = db.VenueImages.Where(img => img.VenueId == id).ToList();
            ViewBag.AddOnServices = db.AddOnServices.Where(a => a.IsActive).ToList();

            return View(venue);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}