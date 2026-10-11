using EventVenueBooking.Database;
using EventVenueBooking.Entities;
using EventVenueBooking.Filters;
using EventVenueBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EventVenueBooking.Controllers
{
    [CustomAuthorize(Roles = "Admin,Coordinator")]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private const byte ClientRole = 0;

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

        public ActionResult Feedback()
        {
            var feedbacks = db.Feedbacks
                .Include(f => f.Booking)
                .Include(f => f.Booking.ClientUser)
                .Include(f => f.Booking.Venue)
                .Select(f => new FeedbackViewModel
                {
                    FeedbackId = f.FeedbackId,
                    BookingId = f.BookingId,
                    CustomerName = f.Booking.ClientUser.FullName,
                    VenueName = f.Booking.Venue.Name,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    SubmittedAt = f.CreatedAt
                })
                .OrderByDescending(f => f.SubmittedAt)
                .ToList();

            return View("~/Views/Admin/Feedback.cshtml", feedbacks);
        }

        // ================= TẠO MỚI USER TỪ POPUP =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateClient(string fullName, string email, string phone, string password)
        {
            fullName = (fullName ?? "").Trim();
            email = (email ?? "").Trim();
            phone = (phone ?? "").Trim();

            if (fullName.Length == 0 || email.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập họ tên và email.";
                return RedirectToAction("Index");
            }
            else if (!IsValidEmail(email))
            {
                TempData["ErrorMessage"] = "Email không hợp lệ.";
                return RedirectToAction("Index");
            }
            else if (db.Users.Any(u => u.Email == email))
            {
                TempData["ErrorMessage"] = "Email này đã được sử dụng.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                TempData["ErrorMessage"] = "Mật khẩu phải có ít nhất 6 ký tự.";
                return RedirectToAction("Index");
            }

            int role = 0;
            if (int.TryParse(Request.Form["role"], out int r)) role = r;

            string isActiveForm = Request.Form["isActive"];
            bool activeStatus = string.IsNullOrEmpty(isActiveForm) || (isActiveForm == "true" || isActiveForm == "True" || isActiveForm == "1" || isActiveForm == "on");

            var user = new User
            {
                FullName = fullName,
                Email = email,
                Phone = phone.Length == 0 ? null : phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = (byte)role,
                IsActive = activeStatus,
                CreatedAt = DateTime.Now
            };
            db.Users.Add(user);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đã tạo tài khoản thành công.";
            return RedirectToAction("Index");
        }

        // ================= CẬP NHẬT TỪ POPUP =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditClient(int userId, string fullName, string email, string phone, string password)
        {
            var user = db.Users.Find(userId);
            if (user == null) return HttpNotFound();

            fullName = (fullName ?? "").Trim();
            email = (email ?? "").Trim();
            phone = (phone ?? "").Trim();

            if (fullName.Length == 0 || email.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập họ tên và email.";
                return RedirectToAction("Index");
            }
            else if (!IsValidEmail(email))
            {
                TempData["ErrorMessage"] = "Email không hợp lệ.";
                return RedirectToAction("Index");
            }
            else if (db.Users.Any(u => u.Email == email && u.UserId != userId))
            {
                TempData["ErrorMessage"] = "Email này đã được sử dụng.";
                return RedirectToAction("Index");
            }

            // Đọc trực tiếp Role và Status từ Request.Form
            int role = 0;
            if (int.TryParse(Request.Form["role"], out int r)) role = r;

            string isActiveForm = Request.Form["isActive"];
            bool activeStatus = (isActiveForm == "true" || isActiveForm == "True" || isActiveForm == "1" || isActiveForm == "on");

            // Kiểm tra an toàn bảo vệ Admin cuối cùng
            if (user.Role == 2 && (role != 2 || !activeStatus))
            {
                int activeAdminCount = db.Users.Count(u => u.Role == 2 && u.IsActive);
                if (activeAdminCount <= 1)
                {
                    TempData["ErrorMessage"] = "Không thể thay đổi quyền hoặc khóa tài khoản của Admin duy nhất còn lại.";
                    return RedirectToAction("Index");
                }
            }

            user.FullName = fullName;
            user.Email = email;
            user.Phone = phone.Length == 0 ? null : phone;
            user.Role = (byte)role;
            user.IsActive = activeStatus;

            if (!string.IsNullOrEmpty(password))
            {
                if (password.Length < 6)
                {
                    TempData["ErrorMessage"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                    return RedirectToAction("Index");
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Đã cập nhật thông tin tài khoản thành công.";
            return RedirectToAction("Index");
        }

        private static bool IsValidEmail(string email)
        {
            try { var m = new System.Net.Mail.MailAddress(email); return m.Address == email; }
            catch { return false; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}