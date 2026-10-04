using EventVenueBooking.Database;
using EventVenueBooking.Entities;
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

        //nhớ thêm authorize Admin
        // GET: Users/ChangeRole
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

        //nhớ thêm authorize Admin
        // POST: Users/ChangeRole
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        ///nhớ thêm authorize Admin
        // GET: Users/ChangeIsActive
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
        //nhớ thêm authorize Admin
        // POST: Users/ChangeIsActive
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        //nhớ thêm authorize Admin
        // GET: Users/AdminResetPassword
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

        //nhớ thêm authorize Admin
        // POST: Users/AdminResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
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