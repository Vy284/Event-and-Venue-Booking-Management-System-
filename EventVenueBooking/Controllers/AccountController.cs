using System;
using System.Linq;
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

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            try
            {
                // Validation của ViewModel không hợp lệ
                if (!ModelState.IsValid)
                {
                    TempData["LoginData"] = new LoginViewModel
                    {
                        Email = model.Email
                        // Không lưu Password
                    };

                    TempData["OpenLoginModal"] = true;
                    TempData["ErrorMessage"] = "Thông tin đăng nhập không hợp lệ!";

                    return RedirectToAction("Index", "Home");
                }

                var user = db.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null && user.IsActive &&
                    BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    Session["User"] = user;
                    Session["UserName"] = string.IsNullOrEmpty(user.FullName)
                        ? user.Email
                        : user.FullName;
                    Session["UserRole"] = user.Role;

                    FormsAuthentication.SetAuthCookie(user.Email, false);

                    TempData["SuccessMessage"] = "Đăng nhập thành công! Chào mừng " + (user.FullName ?? user.Email);
                }
                else
                {
                    // Login sai → giữ Email, không giữ Password
                    TempData["LoginData"] = new LoginViewModel
                    {
                        Email = model.Email
                    };

                    TempData["OpenLoginModal"] = true;
                    TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng!";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return View("Error");
            }

            return Redirect(Request.UrlReferrer?.ToString() ?? "/Home/Index");
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            try
            {
                // Lưu lại những thông tin được phép giữ
                var registerData = new RegisterViewModel
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone
                    // Không lưu Password
                    // Không lưu ConfirmPassword
                };

                if (ModelState.IsValid)
                {
                    var isExist = db.Users.Any(u => u.Email == model.Email);

                    if (isExist)
                    {
                        TempData["ErrorMessage"] = "Email đã được đăng ký!";
                        TempData["RegisterData"] = registerData;
                        TempData["OpenRegisterModal"] = true;

                        return RedirectToAction("Index", "Home");
                    }

                    var user = new UserEntity
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        Phone = model.Phone,

                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),

                        Role = 0,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    Session["User"] = user;
                    Session["UserName"] = string.IsNullOrEmpty(user.FullName)
                        ? user.Email
                        : user.FullName;
                    Session["UserRole"] = user.Role;

                    FormsAuthentication.SetAuthCookie(user.Email, false);

                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        "Thông tin đăng ký không hợp lệ, vui lòng kiểm tra lại!";

                    TempData["RegisterData"] = registerData;
                    TempData["OpenRegisterModal"] = true;
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return View("Error");
            }
        }


        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            TempData["SuccessMessage"] = "Đã đăng xuất tài khoản.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        [Authorize]
        public ActionResult Profile()
        {
            try
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
                        VenueImageUrl = db.VenueImages
                                        .Where(img => img.VenueId == b.VenueId && img.IsPrimary)
                                        .Select(img => img.ImageUrl)
                                        .FirstOrDefault()
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return View("Error");
            }
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(string fullName, string phone)
        {
            try
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return View("Error");
            }          
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}