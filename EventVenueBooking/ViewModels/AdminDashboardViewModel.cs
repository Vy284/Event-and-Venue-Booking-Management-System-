using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventVenueBooking.ViewModels
{
    public class AdminDashboardViewModel
    {
        // 3 thẻ trên cùng
        public int UpcomingEvents { get; set; }      // booking chưa hủy, bắt đầu từ bây giờ trở đi
        public int TotalBookings { get; set; }       // tất cả booking
        public decimal TotalRevenue { get; set; }    // tổng tiền đã thu (Payment Completed)

        // Biểu đồ tròn trạng thái booking
        public List<DashboardStatusItem> StatusCounts { get; set; }

        // Biểu đồ cột 8 tháng gần nhất (giá trị booking chưa hủy, theo tháng tạo)
        public List<string> RevenueLabels { get; set; }
        public List<decimal> VenueCostSeries { get; set; }
        public List<decimal> AddOnCostSeries { get; set; }
        public decimal RevenueSeriesTotal { get; set; }

        // Top loại sự kiện
        public List<DashboardEventTypeItem> TopEventTypes { get; set; }

        public AdminDashboardViewModel()
        {
            StatusCounts = new List<DashboardStatusItem>();
            RevenueLabels = new List<string>();
            VenueCostSeries = new List<decimal>();
            AddOnCostSeries = new List<decimal>();
            TopEventTypes = new List<DashboardEventTypeItem>();
        }
    }

    public class DashboardStatusItem
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public double Percent { get; set; }
        public string Color { get; set; }
    }

    public class DashboardEventTypeItem
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public double Percent { get; set; }
    }
}