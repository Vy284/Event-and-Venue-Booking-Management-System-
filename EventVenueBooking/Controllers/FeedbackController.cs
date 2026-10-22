using EventVenueBooking.Database;
using EventVenueBooking.Filters;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace EventVenueBooking.Controllers
{
    [CustomAuthorize(Roles = "Admin,Coordinator")]
    public class FeedbackController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Feedback/Index
        public ActionResult Index(string search, int? rating)
        {
            // Truy vấn Feedback thông qua mối quan hệ Booking -> ClientUser & Venue
            var query = db.Feedbacks
                .Include("Booking.ClientUser")
                .Include("Booking.Venue")
                .AsQueryable();

            // Tìm kiếm theo tên khách hàng hoặc tên sảnh
            if (!string.IsNullOrEmpty(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(f => f.Booking.ClientUser.FullName.ToLower().Contains(s)
                                      || f.Booking.Venue.Name.ToLower().Contains(s));
            }

            // Lọc theo số sao đánh giá (Rating)
            if (rating.HasValue && rating.Value > 0)
            {
                query = query.Where(f => f.Rating == rating.Value);
            }

            var model = query.OrderByDescending(f => f.CreatedAt).ToList();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentRating = rating;

            // Trả về đúng file view Admin Feedback
            return View("~/Views/Admin/Feedback.cshtml", model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}