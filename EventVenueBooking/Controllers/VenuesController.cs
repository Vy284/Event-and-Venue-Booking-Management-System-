using EventVenueBooking.Database;
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace EventVenueBooking.Controllers
{
    public class VenuesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Venues
        public ActionResult Index()
        {
            var venues = db.Venues
                .Include(v => v.VenueType)
                .Include(v => v.VenueImages)
                .Include(v => v.Facilities)
                .ToList()
                .Select(v => new VenueListViewModel
                {
                    VenueId = v.VenueId,
                    VenueName = v.Name,
                    Capacity = v.Capacity,
                    PricePerHour = v.RentalRate,
                    TypeName = v.VenueType != null ? v.VenueType.TypeName : "N/A",
                    PrimaryImageUrl = v.VenueImages.FirstOrDefault(img => img.IsPrimary)?.ImageUrl ?? "/images/default-venue.jpg",
                    IsActive = v.Status == 0, // 0 = Active
                    Facilities = v.Facilities.Select(f => f.FacilityName).ToList()
                }).ToList();

            return View(venues);
        }

        // GET: Venues/Facilities
        public ActionResult Facilities()
        {
            var facilities = db.Facilities.ToList();
            return View(facilities);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}