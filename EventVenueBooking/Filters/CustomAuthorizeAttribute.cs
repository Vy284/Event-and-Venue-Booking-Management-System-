using System.Net;
using System.Web.Mvc;
using System.Web.Routing;

namespace EventVenueBooking.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response;

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // Đã đăng nhập nhưng sai role -> 403 kèm trang thông báo, không đá về Login (tránh vòng lặp)
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.TrySkipIisCustomErrors = true;   // để IIS không thay bằng trang lỗi mặc định

                filterContext.Result = new ContentResult
                {
                    ContentType = "text/html; charset=utf-8",
                    Content = "<div style='font-family:Segoe UI,sans-serif;text-align:center;margin-top:80px'>" +
                              "<h2>403 - Không có quyền truy cập</h2>" +
                              "<p>Bạn không có quyền truy cập trang này.</p>" +
                              "<a href='javascript:history.back()'>Quay lại</a></div>"
                };
            }
            else if (request.IsAjaxRequest())
            {
                // Gọi bằng AJAX/fetch: trả JSON để JS tự mở modal đăng nhập
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.TrySkipIisCustomErrors = true;
                response.SuppressFormsAuthenticationRedirect = true;   // không cho Forms Auth đổi 401 thành redirect

                filterContext.Result = new JsonResult
                {
                    Data = new { success = false, requireLogin = true, message = "Vui lòng đăng nhập để tiếp tục." },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                // Chưa đăng nhập -> về trang chủ, mở modal đăng nhập (không có Account/Login.cshtml)
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "Index",
                    showLogin = true,
                    returnUrl = request.RawUrl
                }));
            }
        }
    }
}