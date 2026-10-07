using EventVenueBooking.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace EventVenueBooking
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // TỰ ĐỘNG NẠP ROLE TỪ FORMSAUTHENTICATION COOKIE VÀO HTTPCONTEXT.USER
        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
            {
                try
                {
                    var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    if (authTicket != null && !authTicket.Expired)
                    {
                        // Lấy chuỗi Role đã mã hóa trong UserData (Client, Admin, Coordinator)
                        string[] roles = authTicket.UserData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        // Thiết lập GenericPrincipal mới chứa Identity và Roles cho HttpContext
                        var id = new GenericIdentity(authTicket.Name, "Forms");
                        Context.User = new GenericPrincipal(id, roles);
                    }
                }
                catch
                {
                    // Tránh crash ứng dụng nếu Cookie bị lỗi hỏng hoặc hếtt hạn
                }
            }
        }
    }
}