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
    // Coordinator (lễ tân) xem được danh sách Users, tạo/sửa tài khoản Client.
    // Đổi role, bật/tắt tài khoản, reset mật khẩu chỉ Admin (attribute riêng ở từng action).
    [CustomAuthorize(Roles = "Admin,Coordinator")]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private const byte ClientRole = 0;

        // GET: Users  (SỬA: Coordinator cũng xem được)
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

        // ================= THÊM: Coordinator/Admin tạo và sửa tài khoản CLIENT =================

        // GET: Users/CreateClient
        public ActionResult CreateClient()
        {
            return View(new User());
        }

        // POST: Users/CreateClient
        // Role luôn là Client do server gán, không nhận role từ form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateClient(string fullName, string email, string phone, string password)
        {
            fullName = (fullName ?? "").Trim();
            email = (email ?? "").Trim();
            phone = (phone ?? "").Trim();

            var model = new User { FullName = fullName, Email = email, Phone = phone };

            if (fullName.Length == 0 || email.Length == 0)
                ModelState.AddModelError("", "Vui lòng nhập họ tên và email.");
            else if (!IsValidEmail(email))
                ModelState.AddModelError("", "Email không hợp lệ.");
            else if (db.Users.Any(u => u.Email == email))
                ModelState.AddModelError("", "Email này đã được sử dụng.");

            if (string.IsNullOrEmpty(password) || password.Length < 6)
                ModelState.AddModelError("", "Mật khẩu tạm phải có ít nhất 6 ký tự.");

            if (!ModelState.IsValid)
                return View(model);

            var client = new User
            {
                FullName = fullName,
                Email = email,
                Phone = phone.Length == 0 ? null : phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = ClientRole,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            db.Users.Add(client);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đã tạo tài khoản khách hàng.";
            return RedirectToAction("Index");
        }

        // GET: Users/EditClient/5
        public ActionResult EditClient(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            if (user.Role != ClientRole)
            {
                TempData["ErrorMessage"] = "Chỉ được sửa thông tin tài khoản khách hàng (Client).";
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // POST: Users/EditClient
        // Chỉ sửa họ tên, email, SĐT của Client. Không đụng tới role, trạng thái, mật khẩu.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditClient(int userId, string fullName, string email, string phone)
        {
            var user = db.Users.Find(userId);
            if (user == null) return HttpNotFound();

            if (user.Role != ClientRole)
            {
                TempData["ErrorMessage"] = "Chỉ được sửa thông tin tài khoản khách hàng (Client).";
                return RedirectToAction("Index");
            }

            fullName = (fullName ?? "").Trim();
            email = (email ?? "").Trim();
            phone = (phone ?? "").Trim();

            var model = new User { UserId = userId, FullName = fullName, Email = email, Phone = phone };

            if (fullName.Length == 0 || email.Length == 0)
                ModelState.AddModelError("", "Vui lòng nhập họ tên và email.");
            else if (!IsValidEmail(email))
                ModelState.AddModelError("", "Email không hợp lệ.");
            else if (db.Users.Any(u => u.Email == email && u.UserId != userId))
                ModelState.AddModelError("", "Email này đã được sử dụng.");

            if (!ModelState.IsValid)
                return View(model);

            user.FullName = fullName;
            user.Email = email;
            user.Phone = phone.Length == 0 ? null : phone;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đã cập nhật thông tin khách hàng.";
            return RedirectToAction("Index");
        }

        private static bool IsValidEmail(string email)
        {
            try { var m = new System.Net.Mail.MailAddress(email); return m.Address == email; }
            catch { return false; }
        }

        // ================= CHỈ ADMIN =================

        // GET: Users/ChangeRole
        [CustomAuthorize(Roles = "Admin")]              // SỬA: trước đây thiếu, Coordinator tự nâng quyền được
        public ActionResult ChangeRole(int id)
        {
            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new ChangeRoleViewModel
            {
                UserId = user.UserId,
                Role = user.Role
            };

            return View(model);
        }

        // POST: Users/ChangeRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult ChangeRole(ChangeRoleViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (model.Role > 2)
                {
                    ModelState.AddModelError(
                        "Role",
                        "Role không hợp lệ.");

                    return View(model);
                }

                var user = db.Users.Find(model.UserId);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Nếu không thực sự thay đổi Role
                if (user.Role == model.Role)
                {
                    TempData["SuccessMessage"] =
                        "Role của tài khoản không thay đổi.";

                    return RedirectToAction("Index");
                }

                // Nếu đang chuyển Admin -> Role khác
                if (user.Role == 2 && model.Role != 2)
                {
                    int activeAdminCount = db.Users.Count(u =>
                        u.Role == 2 &&
                        u.IsActive);

                    // Sau khi đổi Role phải còn ít nhất 2 Active Admin
                    if (activeAdminCount <= 2)
                    {
                        TempData["ErrorMessage"] =
                            "Không thể thay đổi Role. Hệ thống phải luôn có ít nhất 2 Admin đang hoạt động.";

                        return RedirectToAction("Index");
                    }
                }

                user.Role = model.Role;
                db.SaveChanges();

                // Kiểm tra Admin có đang tự đổi Role của mình hay không
                var sessionUser = Session["User"] as User;
                if (sessionUser != null && sessionUser.UserId == user.UserId)
                {
                    // Xóa phiên đăng nhập hiện tại
                    Session.Clear();
                    FormsAuthentication.SignOut();
                    TempData["SuccessMessage"] =
                        "Thay đổi Role thành công. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Index", "Account");
                }

                TempData["SuccessMessage"] =
                    "Thay đổi Role thành công.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return View("Error");
            }
        }

        // GET: Users/ChangeIsActive
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult ChangeIsActive(int id)
        {
            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new ChangeIsActiveViewModel
            {
                UserId = user.UserId,
                IsActive = user.IsActive
            };

            return View(model);
        }

        // POST: Users/ChangeIsActive
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult ChangeIsActive(ChangeIsActiveViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = db.Users.Find(model.UserId);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Không thay đổi gì
                if (user.IsActive == model.IsActive)
                {
                    TempData["SuccessMessage"] =
                        "Trạng thái tài khoản không thay đổi.";

                    return RedirectToAction("Index");
                }

                // Nếu đang deactivate một Active Admin
                if (user.Role == 2 &&
                    user.IsActive &&
                    !model.IsActive)
                {
                    int activeAdminCount = db.Users.Count(u =>
                        u.Role == 2 &&
                        u.IsActive);

                    // Sau khi deactivate phải còn ít nhất 2 Active Admin
                    if (activeAdminCount <= 2)
                    {
                        TempData["ErrorMessage"] =
                            "Không thể vô hiệu hóa tài khoản. Hệ thống phải luôn có ít nhất 2 Admin đang hoạt động.";

                        return RedirectToAction("Index");
                    }
                }

                user.IsActive = model.IsActive;

                db.SaveChanges();

                // Kiểm tra Admin có đang tự thay đổi IsActive của mình hay không
                var sessionUser = Session["User"] as User;

                if (sessionUser != null &&
                    sessionUser.UserId == user.UserId)
                {
                    // Nếu tự deactivate chính mình
                    if (!user.IsActive)
                    {
                        Session.Clear();
                        FormsAuthentication.SignOut();

                        TempData["SuccessMessage"] =
                            "Tài khoản đã được vô hiệu hóa.";

                        return RedirectToAction("Index", "Account");
                    }
                }

                TempData["SuccessMessage"] =
                    model.IsActive
                        ? "Kích hoạt tài khoản thành công."
                        : "Vô hiệu hóa tài khoản thành công.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return View("Error");
            }
        }

        // GET: Users/AdminResetPassword
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult AdminResetPassword(int id)
        {
            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Không cho reset password của Admin
            if (user.Role == 2)
            {
                TempData["ErrorMessage"] =
                    "Không thể reset mật khẩu của tài khoản Admin.";

                return RedirectToAction("Index");
            }

            var model = new AdminResetPasswordViewModel
            {
                UserId = user.UserId
            };

            return View(model);
        }

        // POST: Users/AdminResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]              // SỬA
        public ActionResult AdminResetPassword(AdminResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = db.Users.Find(model.UserId);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Không cho reset password của Admin
                if (user.Role == 2)
                {
                    TempData["ErrorMessage"] =
                        "Không thể reset mật khẩu của tài khoản Admin.";

                    return RedirectToAction("Index");
                }

                user.PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Reset mật khẩu thành công.";

                return RedirectToAction("Index");
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