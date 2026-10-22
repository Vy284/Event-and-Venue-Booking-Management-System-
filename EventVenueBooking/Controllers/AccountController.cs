using EventVenueBooking.Database;
using EventVenueBooking.Filters;
using EventVenueBooking.ViewModels;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UserEntity = EventVenueBooking.Entities.User; // Đặt UserEntity để C# không nhầm với Controller.User

namespace EventVenueBooking.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //Get role
        private string GetRoleName(byte role)
        {
            switch (role)
            {
                case 2:
                    return "Admin";

                case 1:
                    return "Coordinator";

                default:
                    return "Client";
            }
        }

        // Chọn nơi chuyển tới sau khi đăng nhập/đăng ký: returnUrl nội bộ > trang trước đó > trang chủ
        private string GetSafeRedirectUrl(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return returnUrl;

            var referrer = Request.UrlReferrer;
            if (referrer != null
                && referrer.Host == Request.Url.Host
                && referrer.Query.IndexOf("showLogin", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return referrer.ToString();
            }

            return Url.Action("Index", "Home");
        }

        // Lấy người dùng hiện tại theo cookie đăng nhập (User.Identity.Name = Email).
        // Không phụ thuộc Session vì Session (mặc định 20 phút) có thể hết hạn sớm hơn cookie (30 phút).
        private UserEntity GetCurrentUser()
        {
            string email = User.Identity.Name;
            if (string.IsNullOrEmpty(email)) return null;

            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !user.IsActive) return null;

            // Khôi phục Session nếu đã hết hạn
            if (Session["User"] == null)
            {
                Session["User"] = user;
                Session["UserName"] = string.IsNullOrEmpty(user.FullName) ? user.Email : user.FullName;
                Session["UserRole"] = user.Role;
            }
            return user;
        }

        // Chưa đăng nhập: về trang chủ và tự mở popup đăng nhập
        private ActionResult RedirectToLogin(string message, string returnUrl = null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction("Index", "Home", new { showLogin = true, returnUrl = returnUrl });
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    if (Request.IsAjaxRequest())
                        return Json(new { success = false, message = "Thông tin đăng nhập không hợp lệ!" });

                    TempData["LoginData"] = new LoginViewModel { Email = model.Email };
                    TempData["OpenLoginModal"] = true;
                    TempData["ErrorMessage"] = "Thông tin đăng nhập không hợp lệ!";
                    return RedirectToAction("Index", "Home");
                }

                var user = db.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null && user.IsActive && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    Session["User"] = user;
                    Session["UserName"] = string.IsNullOrEmpty(user.FullName) ? user.Email : user.FullName;
                    Session["UserRole"] = user.Role;

                    string roleName = GetRoleName(user.Role);

                    var ticket = new FormsAuthenticationTicket(
                        1, user.Email, DateTime.Now, DateTime.Now.AddMinutes(30), false, roleName, FormsAuthentication.FormsCookiePath
                    );

                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket));

                    TempData["SuccessMessage"] = "Đăng nhập thành công! Chào mừng " + (user.FullName ?? user.Email);

                    string target = GetSafeRedirectUrl(returnUrl);

                    // NẾU LÀ AJAX: Trả về JSON thông báo thành công + link chuyển trang
                    if (Request.IsAjaxRequest())
                        return Json(new { success = true, redirectUrl = target });

                    return Redirect(target);
                }
                else
                {
                    if (Request.IsAjaxRequest())
                        return Json(new { success = false, message = "Email hoặc mật khẩu không đúng!" });

                    TempData["LoginData"] = new LoginViewModel { Email = model.Email };
                    TempData["OpenLoginModal"] = true;
                    TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng!";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                if (Request.IsAjaxRequest())
                    return Json(new { success = false, message = "Lỗi hệ thống, vui lòng thử lại!" });
                return View("Error");
            }

            // Đăng nhập thất bại (không AJAX): quay lại trang trước đó để mở lại modal
            return Redirect(Request.UrlReferrer?.ToString() ?? "/Home/Index");
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model, string returnUrl = null)
        {
            try
            {
                var registerData = new RegisterViewModel
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone
                };

                if (ModelState.IsValid)
                {
                    var isExist = db.Users.Any(u => u.Email == model.Email);

                    if (isExist)
                    {
                        if (Request.IsAjaxRequest())
                            return Json(new { success = false, message = "Email đã được đăng ký!" });

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
                    Session["UserName"] = string.IsNullOrEmpty(user.FullName) ? user.Email : user.FullName;
                    Session["UserRole"] = user.Role;

                    string roleName = GetRoleName(user.Role);
                    var ticket = new FormsAuthenticationTicket(1, user.Email, DateTime.Now, DateTime.Now.AddMinutes(30), false, roleName, FormsAuthentication.FormsCookiePath);
                    Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)));

                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";

                    string target = GetSafeRedirectUrl(returnUrl);

                    if (Request.IsAjaxRequest())
                        return Json(new { success = true, redirectUrl = target });

                    return Redirect(target);
                }
                else
                {
                    if (Request.IsAjaxRequest())
                        return Json(new { success = false, message = "Thông tin đăng ký không hợp lệ!" });

                    TempData["ErrorMessage"] = "Thông tin đăng ký không hợp lệ, vui lòng kiểm tra lại!";
                    TempData["RegisterData"] = registerData;
                    TempData["OpenRegisterModal"] = true;
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                if (Request.IsAjaxRequest())
                    return Json(new { success = false, message = "Lỗi hệ thống xảy ra!" });
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
        [CustomAuthorize]
        public new ActionResult Profile()
        {
            try
            {
                // Lấy thông tin mới nhất từ DB theo cookie đăng nhập
                var user = GetCurrentUser();
                if (user == null)
                    return RedirectToLogin("Vui lòng đăng nhập để xem tài khoản!", Url.Action("Profile", "Account"));

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
                        VenueId = b.VenueId,
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
        [CustomAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(UpdateProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Thông tin cập nhật không hợp lệ.";
                    return RedirectToAction("Profile");
                }

                var user = GetCurrentUser();
                if (user == null)
                    return RedirectToLogin("Vui lòng đăng nhập để cập nhật thông tin!", Url.Action("Profile", "Account"));

                user.FullName = model.FullName.Trim();

                user.Phone = string.IsNullOrWhiteSpace(model.Phone)
                    ? null
                    : model.Phone.Trim();

                db.SaveChanges();

                // Cập nhật lại Session
                Session["User"] = user;
                Session["UserName"] = user.FullName;

                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return View("Error");
            }
        }


        //test
        [CustomAuthorize]
        public ActionResult TestRole()
        {
            return Content(
                "Authenticated: " + User.Identity.IsAuthenticated +
                " | Name: " + User.Identity.Name +
                " | Admin: " + User.IsInRole("Admin") +
                " | Coordinator: " + User.IsInRole("Coordinator") +
                " | Client: " + User.IsInRole("Client")
            );
        }

        //GET: Account/ChangeEmail
        [CustomAuthorize]
        public ActionResult ChangeEmail()
        {
            return View();
        }

        // POST: Account/ChangeEmail
        [CustomAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeEmail(ChangeEmailViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = GetCurrentUser();
                if (user == null)
                    return RedirectToLogin("Vui lòng đăng nhập để thay đổi email.", Url.Action("ChangeEmail", "Account"));

                string newEmail = model.NewEmail.Trim();

                bool emailExists = db.Users.Any(u =>
                    u.Email == newEmail &&
                    u.UserId != user.UserId);

                if (emailExists)
                {
                    ModelState.AddModelError("NewEmail", "Email này đã được sử dụng.");
                    return View(model);
                }

                user.Email = newEmail;

                db.SaveChanges();

                Session["User"] = user;

                // Tạo lại authentication ticket vì Email đang
                // được dùng làm Name của FormsAuthenticationTicket.
                string roleName = GetRoleName(user.Role);

                var ticket = new FormsAuthenticationTicket(
                    1,
                    user.Email,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    false,
                    roleName
                );

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                var cookie = new HttpCookie(
                    FormsAuthentication.FormsCookieName,
                    encryptedTicket);

                Response.Cookies.Add(cookie);

                TempData["SuccessMessage"] = "Thay đổi email thành công!";

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return View("Error");
            }
        }

        // GET: Account/ChangePassword
        // Không có trang riêng: form đổi mật khẩu nằm trong Profile.cshtml
        [CustomAuthorize]
        public ActionResult ChangePassword()
        {
            return RedirectToAction("Profile");
        }

        // POST: Account/ChangePassword
        [CustomAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    string firstError = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .FirstOrDefault(m => !string.IsNullOrEmpty(m));

                    TempData["ErrorMessage"] = firstError ?? "Thông tin đổi mật khẩu không hợp lệ.";
                    return RedirectToAction("Profile");
                }

                var user = GetCurrentUser();
                if (user == null)
                    return RedirectToLogin("Vui lòng đăng nhập để thay đổi mật khẩu.", Url.Action("Profile", "Account"));

                bool currentPasswordCorrect =
                    BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);

                if (!currentPasswordCorrect)
                {
                    TempData["ErrorMessage"] = "Mật khẩu hiện tại không chính xác.";
                    return RedirectToAction("Profile");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                db.SaveChanges();

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "Lỗi hệ thống, vui lòng thử lại!";
                return RedirectToAction("Profile");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}