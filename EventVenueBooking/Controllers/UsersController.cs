using EventVenueBooking.Database;
using EventVenueBooking.Entities;
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventVenueBooking.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Users
        public ActionResult Index()
        {
            var users = db.Users.Select(u => new UserViewModel
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                RoleCode = u.Role,
                Role = u.Role == 2 ? "Admin" : (u.Role == 1 ? "Coordinator" : "Client"),
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();

            return View("~/Views/Admin/Users.cshtml", users);
        }

        // GET: Users/Feedback
        public ActionResult Feedback()
        {
            var feedbacks = db.Feedbacks
                .Include(f => f.Booking)
                .Include(f => f.Booking.ClientUser)
                .Include(f => f.Booking.Venue)
                .Select(f => new FeedbackViewModel
                {
                    FeedbackId = f.FeedbackId,
                    CustomerName = f.Booking.ClientUser.FullName,
                    VenueName = f.Booking.Venue.Name,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    SubmittedAt = f.CreatedAt
                }).ToList();

            return View(feedbacks);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}