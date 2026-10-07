using EventVenueBooking.Filters;
using System.Web;
using System.Web.Mvc;

namespace EventVenueBooking
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new CustomAuthorizeAttribute());   // mọi trang phải đăng nhập
        }
    }
}