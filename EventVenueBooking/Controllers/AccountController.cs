using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using EventVenueBooking.Database;
using UserEntity = EventVenueBooking.Entities.User; // Đặt UserEntity để C# không nhầm với Controller.User
namespace EventVenueBooking.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string usernameOrEmail, string password)
        {
            // Email login
            var user = db.Users.FirstOrDefault(u =>
                u.Email == usernameOrEmail
                && u.PasswordHash == password);

            if (user != null)
            {
                Session["User"] = user;
                Session["UserName"] = string.IsNullOrEmpty(user.FullName) ? user.Email : user.FullName;
                Session["UserRole"] = user.Role; // 0 = Client, 1 = Coordinator, 2 = Admin

                FormsAuthentication.SetAuthCookie(user.Email, false);

                TempData["SuccessMessage"] = "Đăng nhập thành công! Chào mừng " + (user.FullName ?? user.Email);
            }
            else
            {
                TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng!";
            }

            return Redirect(Request.UrlReferrer?.ToString() ?? "/Home/Index");
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserEntity model)
        {
            if (ModelState.IsValid)
            {
                var isExist = db.Users.Any(u => u.Email == model.Email);
                if (isExist)
                {
                    TempData["ErrorMessage"] = "Địa chỉ Email này đã được đăng ký!";
                    return Redirect(Request.UrlReferrer?.ToString() ?? "/Home/Index");
                }

                model.Role = 0;
                model.CreatedAt = DateTime.Now;

                db.Users.Add(model);
                db.SaveChanges();

                // Tự động login sau khi đăng ký
                Session["User"] = model;
                Session["UserName"] = string.IsNullOrEmpty(model.FullName) ? model.Email : model.FullName;
                Session["UserRole"] = model.Role;

                FormsAuthentication.SetAuthCookie(model.Email, false);

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Thông tin đăng ký không hợp lệ, vui lòng kiểm tra lại!";
            }

            return Redirect(Request.UrlReferrer?.ToString() ?? "/Home/Index");
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            TempData["SuccessMessage"] = "Đã đăng xuất tài khoản.";
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}