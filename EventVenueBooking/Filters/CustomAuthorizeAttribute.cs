using System.Net;
using System.Web.Mvc;

namespace EventVenueBooking.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // Đã đăng nhập nhưng sai role -> 403 kèm trang thông báo, không đá về Login (tránh vòng lặp)
                var response = filterContext.HttpContext.Response;
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
            else
            {
                base.HandleUnauthorizedRequest(filterContext); // chưa đăng nhập -> về Login
            }
        }
    }
}