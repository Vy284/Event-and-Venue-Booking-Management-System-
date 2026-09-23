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

        public ActionResult Index(string search, string category)
        {
            var query = db.Venues.AsQueryable();

            // Lọc theo Tên sảnh hoặc Vị trí
            if (!string.IsNullOrEmpty(search))
            {
                string searchLower = search.Trim().ToLower();
                query = query.Where(v => v.Name.ToLower().Contains(searchLower) || v.Location.ToLower().Contains(searchLower));
            }

            // Lọc theo Category dựa trên VenueType và EventType
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                string categoryLower = category.Trim().ToLower();

                if (categoryLower == "wedding")
                {
                    // Đám cưới -> Ballroom / Garden
                    query = query.Where(v => v.VenueType.TypeName.ToLower().Contains("ballroom")
                                          || v.VenueType.TypeName.ToLower().Contains("garden")
                                          || v.Name.ToLower().Contains("wedding"));
                }
                else if (categoryLower == "corporate")
                {
                    // Hội thảo -> Ballroom / Indoor
                    query = query.Where(v => v.VenueType.TypeName.ToLower().Contains("ballroom")
                                          || v.VenueType.TypeName.ToLower().Contains("indoor")
                                          || v.Name.ToLower().Contains("corporate"));
                }
                else if (categoryLower == "birthday")
                {
                    // Sinh nhật -> Rooftop / Garden
                    query = query.Where(v => v.VenueType.TypeName.ToLower().Contains("rooftop")
                                          || v.VenueType.TypeName.ToLower().Contains("garden"));
                }
                else if (categoryLower == "social")
                {
                    // Tiệc -> Rooftop / Garden
                    query = query.Where(v => v.VenueType.TypeName.ToLower().Contains("rooftop")
                                          || v.VenueType.TypeName.ToLower().Contains("lounge"));
                }
                else
                {
                    // Trường hợp tìm trực tiếp theo loại DB
                    query = query.Where(v => v.VenueType.TypeName.ToLower().Contains(category.ToLower()));
                }
            }

            var venues = query.Select(v => new VenueCardViewModel
            {
                VenueId = v.VenueId,
                Name = v.Name,
                TypeName = v.VenueType.TypeName,
                Location = v.Location,
                Capacity = v.Capacity,
                RentalRate = v.RentalRate,
                RentalUnit = v.RentalUnit,
                ImageUrl = db.VenueImages.FirstOrDefault(img => img.VenueId == v.VenueId && img.IsPrimary).ImageUrl
                           ?? "/Content/images/bg_landingpage.jpg" // Lấy ảnh trong images nếu không tìm được ảnh hiển thị
            }).ToList();

            // Lưu lại để giữ trạng thái trên View
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = string.IsNullOrEmpty(category) ? "All" : category;

            return View(venues);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}