using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

//namespace EventVenueBooking.Controllers
//{
//    public class HomeController : Controller
//    {
//        public ActionResult Index()
//        {
//            return View();
//        }

//        public ActionResult About()
//        {
//            ViewBag.Message = "Your application description page.";

//            return View();
//        }

//        public ActionResult Contact()
//        {
//            ViewBag.Message = "Your contact page.";

//            return View();
//        }
//    }
//}

namespace EventVenueBooking.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var mockVenues = new List<VenueCardViewModel>
            {
                new VenueCardViewModel {
                    VenueId = 1,
                    Name = "Grand Ballroom Diamond",
                    TypeName = "Weddings",
                    Capacity = 500,
                    RentalRate = 25000000,
                    RentalUnit = 1, // 1 = Session (Buổi)
                    Location = "Tầng 3, Sảnh A",
                    ImageUrl = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?auto=format&fit=crop&w=800&q=80"
                },
                new VenueCardViewModel {
                    VenueId = 2,
                    Name = "Crystal Conference Hall",
                    TypeName = "Corporate",
                    Capacity = 200,
                    RentalRate = 3000000,
                    RentalUnit = 0, // 0 = Hour (Giờ)
                    Location = "Tầng 2, Sảnh B",
                    ImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865?auto=format&fit=crop&w=800&q=80"
                },
                new VenueCardViewModel {
                    VenueId = 3,
                    Name = "Sunset Garden Party",
                    TypeName = "Birthday",
                    Capacity = 150,
                    RentalRate = 18000000,
                    RentalUnit = 2, // 2 = Day (Ngày)
                    Location = "Khuôn viên ngoài trời",
                    ImageUrl = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?auto=format&fit=crop&w=800&q=80"
                }
            };

            return View(mockVenues);
        }
    }
}