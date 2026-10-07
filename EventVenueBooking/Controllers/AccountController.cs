using System;
using System.Linq;
using System.Web;                                   // THÊM: để dùng HttpCookie
using System.Web.Mvc;
using System.Web.Security;
using EventVenueBooking.Database;
using UserEntity = EventVenueBooking.Entities.User; // Đặt UserEntity để C# không nhầm với Controller.User
using EventVenueBooking.ViewModels;

namespace EventVenueBooking.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // THÊM: tạo cookie đăng nhập có kèm tên role trong ticket
        // (Global.asax đọc ticket.UserData để biết role của người dùng)
        private void SignIn(UserEntity user)
        {
            string roleName = user.Role == 2 ? "Admin" : (user.Role == 1 ? "Coordinator" : "Client");

            var ticket = new FormsAuthenticationTicket(
                1, user.Email, DateTime.Now, DateTime.Now.AddMinutes(60), false, roleName);

            string encrypted = FormsAuthentication.Encrypt(ticket);
            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encrypted)
            {
                HttpOnly = true
            });
        }

        // POST: Account/Login
        [AllowAnonymous]                            // THÊM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string usernameOrEmail, string password)
        {
            // Email login
            var user = db.Users.FirstOrDefault(u =>
                u.Email == usernameOrEmail
                && u.PasswordHash == password
                && u.IsActive);                     // THÊM: tài khoản bị khóa thì không đăng nhập được

            if (user != null)
            {
                Session["User"] = user;
                Session["UserName"] = string.IsNullOrEmpty(user.FullName) ? user.Email : user.FullName;
                Session["UserRole"] = user.Role; // 0 = Client, 1 = Coordinator, 2 = Admin

                SignIn(user);                       // SỬA: thay cho FormsAuthentication.SetAuthCookie(...)

                TempData["SuccessMessage"] = "Đăng nhập thành công! Chào mừng " + (user.FullName ?? user.Email);

                // THÊM: Admin / Coordinator vào thẳng trang quản trị
                if (user.Role == 1 || user.Role == 2)
                    return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng!";
            }

            return Redirect(Request.UrlReferrer?.ToString() ?? "/Home/Index");
        }

        // POST: Account/Register
        [AllowAnonymous]                            // THÊM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserEntity model)
        {
            if (ModelState.IsValid)
            {
                var isExist = db.Users.Any(u => u.Email == model.Email);
                if (isExist)
                {
                    TempData["ErrorMessage"] = "Email đã được đăng ký!";
                    TempData["RegisterData"] = model;
                    TempData["OpenRegisterModal"] = true;
                    return Redirect(Request.UrlReferrer?.ToString() ?? "/Home/Index");
                }

                model.Role = 0;
                model.CreatedAt = DateTime.Now;

                db.Users.Add(model);
                db.SaveChanges();

                // Tự động login
                Session["User"] = model;
                Session["UserName"] = string.IsNullOrEmpty(model.FullName) ? model.Email : model.FullName;
                Session["UserRole"] = model.Role;

                SignIn(model);                      // SỬA: thay cho FormsAuthentication.SetAuthCookie(...)

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Thông tin đăng ký không hợp lệ, vui lòng kiểm tra lại!";
            }

            return Redirect(Request.UrlReferrer?.ToString() ?? "/Home/Index");
        }

        // GET/POST: Account/Logout
        [AllowAnonymous]                            // THÊM
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            TempData["SuccessMessage"] = "Đã đăng xuất tài khoản.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        public ActionResult Profile()
        {
            // Kiểm tra đăng nhập qua Session
            var sessionUser = Session["User"] as UserEntity;
            if (sessionUser == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem trang cá nhân!";
                return RedirectToAction("Index", "Home");
            }

            // Lấy thông tin mới nhất từ DB
            var user = db.Users.FirstOrDefault(u => u.UserId == sessionUser.UserId);
            if (user == null) return HttpNotFound();

            string roleDisplay = user.Role == 2 ? "Quản Trị Viên (Admin)" :
                                 user.Role == 1 ? "Nhân Viên (Coordinator)" : "Khách Hàng Thân Thiết";

            var model = new ProfileViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                RoleName = roleDisplay,
                CreatedAt = user.CreatedAt,
                Bookings = user.Bookings.OrderByDescending(b => b.CreatedAt).Select(b => new UserBookingViewModel
                {
                    BookingId = b.BookingId,
                    VenueName = b.Venue != null ? b.Venue.Name : "Sảnh sự kiện",
                    VenueImageUrl = db.VenueImages.FirstOrDefault(img => img.VenueId == b.VenueId && img.IsPrimary).ImageUrl
                                   ?? "/Content/images/bg_landingpage.jpg",
                    EventTypeName = b.EventType != null ? b.EventType.TypeName : "Tổ chức sự kiện",
                    EventStartDateTime = b.EventStartDateTime,
                    EventEndDateTime = b.EventEndDateTime,
                    GuestCount = b.GuestCount,
                    TotalCost = b.TotalCost,
                    Status = b.Status
                }).ToList()
            };

            return View(model);
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(string fullName, string phone)
        {
            var sessionUser = Session["User"] as UserEntity;
            if (sessionUser == null) return RedirectToAction("Index", "Home");

            var user = db.Users.Find(sessionUser.UserId);
            if (user != null)
            {
                user.FullName = fullName;
                user.Phone = phone;
                db.SaveChanges();

                // Cập nhật lại Session
                Session["User"] = user;
                Session["UserName"] = user.FullName;

                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
            }

            return RedirectToAction("Profile");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}