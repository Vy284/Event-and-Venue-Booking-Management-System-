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

        protected void Application_PostAuthenticateRequest()
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie == null)
                return;

            var ticket = FormsAuthentication.Decrypt(authCookie.Value);

            if (ticket == null || ticket.Expired)
                return;

            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.FirstOrDefault(u =>
                    u.Email == ticket.Name);

                if (user == null || !user.IsActive)
                {
                    FormsAuthentication.SignOut();
                    return;
                }

                string roleName = ticket.UserData;

                var identity = new FormsIdentity(ticket);
                var principal =
                    new GenericPrincipal(identity, new[] { roleName });

                Context.User = principal;
                System.Threading.Thread.CurrentPrincipal = principal;
            }
        }
    }
}
