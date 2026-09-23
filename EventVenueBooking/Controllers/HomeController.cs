using EventVenueBooking.Database;
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventVenueBooking.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var venues = db.Venues.Select(v => new VenueCardViewModel
            {
                VenueId = v.VenueId,
                Name = v.Name,
                TypeName = v.VenueType.TypeName,
                Location = v.Location,
                Capacity = v.Capacity,
                RentalRate = v.RentalRate,
                RentalUnit = v.RentalUnit,
                ImageUrl = db.VenueImages.FirstOrDefault(img => img.VenueId == v.VenueId && img.IsPrimary).ImageUrl
                           ?? "/Content/images/bg_landingpage.jpg"  // Lấy ảnh trong images nếu không tìm được ảnh hiển thị
            }).ToList();

            return View(venues);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}