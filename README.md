# Event & Venue Booking Management System 🏢🎉

Hệ thống quản lý và đặt lịch sự kiện / địa điểm (Event & Venue Booking Management System) được xây dựng trên nền tảng **ASP.NET MVC** và **Entity Framework 6 (Code First)**.

---

## 🛠️ Công nghệ sử dụng
- **Framework:** .NET Framework / ASP.NET MVC 5
- **ORM:** Entity Framework 6 (Code First) - NuGet package 6.5.2
- **Database:** SQL Server / LocalDB
- **Language:** C#

---

## 🗄️ Cấu trúc Cơ sở dữ liệu (12 Tables)
Hệ thống bao gồm 12 thực thể chính:
1. **Users:** Quản lý người dùng (Client, Staff, Manager, Admin).
2. **VenueTypes:** Phân loại địa điểm / sảnh.
3. **Venues:** Thông tin sảnh / địa điểm tổ chức.
4. **VenueImages:** Hình ảnh của các địa điểm.
5. **Facilities:** Danh mục tiện ích (Âm thanh, màn hình LED, Wi-Fi...).
6. **VenueFacilities:** Bảng nối N-N giữa Venue và Facility.
7. **EventTypes:** Loại hình sự kiện (Tiệc cưới, Hội thảo, Sinh nhật...).
8. **AddOnServices:** Các dịch vụ đi kèm (MC, Ca sĩ, Trang trí...).
9. **Bookings:** Thông tin đơn đặt sân / sảnh.
10. **BookingAddOns:** Chi tiết các dịch vụ thêm của đơn đặt.
11. **Payments:** Lịch sử thanh toán.
12. **Feedbacks:** Đánh giá & phản hồi từ khách hàng.

---

## 🚀 Hướng dẫn thiết lập dự án, database và lưu ý cho thành viên (Setup Guide)

Sau khi **Clone** hoặc **Pull** code mới nhất từ repository về máy cá nhân, các thành viên thực hiện các bước sau để dựng Database:

### 1. Mỗi khi bật project:
- Luôn luôn dùng lệnh pull để lấy code của banch "Main" mới nhất

### 2. Để luôn có dữ liệu mới và phiên database mới nhất (chỉ lần đầu tải xuống)
Kiểm tra cấu hình Chuỗi kết nối (Connection String): Mở file `Web.config` và kiểm tra `connectionString` trong thẻ `DefaultConnection` 
cho phù hợp với SQL Server Instance trên máy:
```xml
<connectionStrings>
  <add name="EventVenueBooking" 
       connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=EventVenueBookingDB;Integrated Security=True;TrustServerCertificate=True;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>

=> Mọi người pull code mới về nhé. Trong file Web.config mình đã để mặc định chuỗi kết nối LocalDB.
Ai dùng SQL Server / SQLEXPRESS thì chỉ cần mở Web.config đổi lại Data Source tên server của máy mình rồi bấm F5 chạy web là DB tự động tạo nhé!
 
