namespace EventVenueBooking.Migrations
{
    using EventVenueBooking.Entities;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<EventVenueBooking.Database.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        // NGUYÊN TẮC CỦA SEED NÀY
        // Các cột Id (UserId, VenueId, BookingId...) là IDENTITY nên SQL Server tự cấp số,
        // EF bỏ qua Id gán tay khi INSERT. Vì vậy AddOrUpdate không được "tìm dòng cũ" bằng Id:
        //   - Bảng có cột UNIQUE (Email, TypeName, FacilityName, Name) -> dùng cột đó làm khóa.
        //   - Venues: dùng Name. VenueImages / Bookings / Payments: dùng bộ cột nghiệp vụ.
        // Sau khi lưu, Id thật được tra lại để gắn khóa ngoại cho bảng sau.
        // Nhờ vậy chạy Seed bao nhiêu lần cũng không sinh dòng trùng.
        protected override void Seed(EventVenueBooking.Database.ApplicationDbContext context)
        {
            DateTime seedDate = new DateTime(2026, 9, 1);

            // ==================== USERS ====================
            const string adminHash = "$2a$11$0VIDReRhGnqWLo6SWTBKI.y8Y8dDKt1ba1sOgv7quUmI9L2352SFm"; // BCrypt "admin123"
            const string userHash = "$2a$11$tZVy9U9SCAIyrkJl7o194.9KrwMZ7k3IAlFD8QiwO6pXruJHQMPx2";  // BCrypt "user123"

            context.Users.AddOrUpdate(u => u.Email,
                new User { FullName = "Quản Lý Hệ Thống", Email = "admin@eventvenue.com", PasswordHash = adminHash, Phone = "0901234567", Role = 2, IsActive = true, CreatedAt = seedDate },
                new User { FullName = "Nhân Viên Lễ Tân", Email = "receptionist@eventvenue.com", PasswordHash = userHash, Phone = "0909876543", Role = 1, IsActive = true, CreatedAt = seedDate },
                new User { FullName = "Nguyễn Minh Anh", Email = "minhanh@example.com", PasswordHash = userHash, Phone = "0911000001", Role = 0, IsActive = true, CreatedAt = new DateTime(2026, 9, 1) },
                new User { FullName = "Trần Hoàng Nam", Email = "hoangnam@example.com", PasswordHash = userHash, Phone = "0911000002", Role = 0, IsActive = true, CreatedAt = new DateTime(2026, 9, 2) },
                new User { FullName = "Lê Thu Hà", Email = "thuha@example.com", PasswordHash = userHash, Phone = "0911000003", Role = 0, IsActive = true, CreatedAt = new DateTime(2026, 9, 3) }
            );
            context.SaveChanges();

            Func<string, int> userId = email => context.Users.Single(u => u.Email == email).UserId;
            int adminId = userId("admin@eventvenue.com");
            int coordinatorId = userId("receptionist@eventvenue.com");
            int minhAnhId = userId("minhanh@example.com");
            int hoangNamId = userId("hoangnam@example.com");
            int thuHaId = userId("thuha@example.com");

            // ==================== MASTER DATA ====================
            const string tBallroom = "Sảnh Trong Nhà (Indoor Ballroom)";
            const string tGarden = "Sân Vườn (Outdoor Garden)";
            const string tRooftop = "Rooftop / Tầng Thượng";
            const string tConference = "Phòng Hội Nghị (Conference Hall)";

            context.VenueTypes.AddOrUpdate(vt => vt.TypeName,
                new VenueType { TypeName = tBallroom, IsActive = true },
                new VenueType { TypeName = tGarden, IsActive = true },
                new VenueType { TypeName = tRooftop, IsActive = true },
                new VenueType { TypeName = tConference, IsActive = true }
            );

            context.Facilities.AddOrUpdate(f => f.FacilityName,
                new Facility { FacilityName = "Màn hình LED 4K cỡ lớn", IsActive = true },
                new Facility { FacilityName = "Hệ thống Âm thanh / Ánh sáng", IsActive = true },
                new Facility { FacilityName = "Sân khấu di động", IsActive = true },
                new Facility { FacilityName = "Máy chiếu & Màn chiếu HD", IsActive = true },
                new Facility { FacilityName = "Hệ thống Điều hòa công suất lớn", IsActive = true },
                new Facility { FacilityName = "Wifi tốc độ cao", IsActive = true },
                new Facility { FacilityName = "Bãi đỗ xe ô tô / xe máy", IsActive = true }
            );

            const string eWedding = "Tiệc Cưới (Wedding)";
            const string eConference = "Hội Thảo / Conference";
            const string eBirthday = "Tiệc Sinh Nhật / Anniversary";
            const string eGala = "Sự Kiện Doanh Nghiệp / Gala Dinner";
            const string eLaunch = "Lễ Ra Mắt Sản Phẩm";

            context.EventTypes.AddOrUpdate(et => et.TypeName,
                new EventType { TypeName = eWedding, IsActive = true },
                new EventType { TypeName = eConference, IsActive = true },
                new EventType { TypeName = eBirthday, IsActive = true },
                new EventType { TypeName = eGala, IsActive = true },
                new EventType { TypeName = eLaunch, IsActive = true }
            );

            context.AddOnServices.AddOrUpdate(a => a.Name,
                new AddOnService { Name = "MC Dẫn Chương Trình", Description = "MC chuyên nghiệp dẫn tiệc cưới, sự kiện", Price = 2000000, Category = "Nhân sự", IsActive = true },
                new AddOnService { Name = "Trang Trí Hoa Tươi", Description = "Trang trí bàn tiệc và cổng chào theo yêu cầu", Price = 5000000, Category = "Trang trí", IsActive = true },
                new AddOnService { Name = "Ban Nhạc Acoustic", Description = "Ban nhạc biểu diễn giao lưu trong buổi tiệc", Price = 3500000, Category = "Giải trí", IsActive = true },
                new AddOnService { Name = "Chụp Ảnh / Quay Phim", Description = "Thợ chụp và quay phim chuyên nghiệp", Price = 4000000, Category = "Truyền thông", IsActive = true }
            );
            context.SaveChanges();

            var venueTypeId = context.VenueTypes.ToDictionary(t => t.TypeName, t => t.VenueTypeId);
            var eventTypeId = context.EventTypes.ToDictionary(t => t.TypeName, t => t.EventTypeId);

            // ==================== VENUES ====================
            // RentalUnit = 0 (Hour): RentalRate là giá MỖI GIỜ.
            const string vGrand = "Sảnh Grand Ballroom";
            const string vGarden = "Sân Vườn Garden Eden";
            const string vSky = "Rooftop Sky Lounge";
            const string vLotus = "Phòng Hội Nghị Lotus";

            context.Venues.AddOrUpdate(v => v.Name,
                new Venue { Name = vGrand, VenueTypeId = venueTypeId[tBallroom], Capacity = 500, Description = "Sảnh tiệc sang trọng trần cao.", RentalRate = 15000000, RentalUnit = 0, Location = "Tầng 1 - Khu A", Status = 1, CreatedAt = seedDate, CreatedByUserId = adminId },
                new Venue { Name = vGarden, VenueTypeId = venueTypeId[tGarden], Capacity = 300, Description = "Không gian sân vườn thoáng đãng.", RentalRate = 12000000, RentalUnit = 0, Location = "Khu Sân Vườn", Status = 1, CreatedAt = seedDate, CreatedByUserId = adminId },
                new Venue { Name = vSky, VenueTypeId = venueTypeId[tRooftop], Capacity = 150, Description = "Tầng thượng view thành phố.", RentalRate = 2000000, RentalUnit = 0, Location = "Tầng 12", Status = 1, CreatedAt = seedDate, CreatedByUserId = adminId },
                new Venue { Name = vLotus, VenueTypeId = venueTypeId[tConference], Capacity = 100, Description = "Phòng họp trang bị hiện đại.", RentalRate = 800000, RentalUnit = 0, Location = "Tầng 2", Status = 1, CreatedAt = seedDate, CreatedByUserId = adminId }
            );
            context.SaveChanges();

            Func<string, int> venueId = name => context.Venues.Where(v => v.Name == name).OrderBy(v => v.VenueId).Select(v => v.VenueId).First();
            int grandId = venueId(vGrand);
            int gardenId = venueId(vGarden);
            int skyId = venueId(vSky);
            int lotusId = venueId(vLotus);

            // ==================== VENUE IMAGES ====================
            // 4 ảnh mỗi venue, đúng 1 ảnh chính. URL tạm, thay lại sau nếu cần.
            const string unsplash = "https://images.unsplash.com/photo-";
            Func<int, string, bool, VenueImage> image = (vid, url, primary) =>
                new VenueImage { VenueId = vid, ImageUrl = unsplash + url, IsPrimary = primary };

            context.VenueImages.AddOrUpdate(i => new { i.VenueId, i.ImageUrl },
                // Grand Ballroom
                image(grandId, "1519167758481-83f550bb49b3?w=800", true),
                image(grandId, "1511578314322-379afb476865?w=800", false),
                image(grandId, "1507504031003-b417219a0fde?w=800", false),
                image(grandId, "1464366400600-7168b8af9bc3?w=800", false),
                // Garden Eden
                image(gardenId, "1587271407850-8d438ca9fdf2?w=800", true),
                image(gardenId, "1519167758481-83f550bb49b3?w=800", false),
                image(gardenId, "1464366400600-7168b8af9bc3?w=800", false),
                image(gardenId, "1507504031003-b417219a0fde?w=800", false),
                // Sky Lounge
                image(skyId, "1533105079780-92b9be482077?w=800", true),
                image(skyId, "1511578314322-379afb476865?w=800", false),
                image(skyId, "1464366400600-7168b8af9bc3?w=800", false),
                image(skyId, "1519167758481-83f550bb49b3?w=800", false),
                // Lotus Conference Hall
                image(lotusId, "1431540015161-0bf868a2d407?w=800", true),
                image(lotusId, "1511578314322-379afb476865?w=800", false),
                image(lotusId, "1507504031003-b417219a0fde?w=800", false),
                image(lotusId, "1464366400600-7168b8af9bc3?w=800", false)
            );
            context.SaveChanges();

            // ==================== DEMO BOOKINGS ====================
            // Status: 0 Pending, 1 Confirmed, 2 InProgress, 3 Completed, 4 Cancelled
            // RentalUnitAtBooking = 0 (tính theo giờ). Mọi booking demo kéo dài 4 giờ -> RentalQuantity = 4.
            // VenueCost = VenueRateAtBooking * RentalQuantity.
            // Khóa tìm dòng: (ClientUserId, VenueId, EventStartDateTime).
            DateTime start1001 = new DateTime(2026, 9, 20, 18, 0, 0);
            DateTime start1002 = new DateTime(2026, 11, 15, 17, 0, 0);
            DateTime start1003 = new DateTime(2026, 10, 10, 18, 0, 0);
            DateTime start1004 = new DateTime(2026, 10, 3, 17, 0, 0);
            DateTime start1005 = new DateTime(2026, 10, 10, 17, 0, 0);
            DateTime start1006 = new DateTime(2026, 11, 20, 8, 0, 0);
            DateTime start1007 = new DateTime(2026, 10, 18, 8, 0, 0);
            DateTime start1008 = new DateTime(2026, 11, 25, 18, 0, 0);

            context.Bookings.AddOrUpdate(b => new { b.ClientUserId, b.VenueId, b.EventStartDateTime },

                // Khách Minh Anh: Completed, Confirmed, Cancelled
                new Booking
                {
                    ClientUserId = minhAnhId,
                    VenueId = grandId,
                    EventTypeId = eventTypeId[eWedding],
                    GuestCount = 200,
                    EventStartDateTime = start1001,
                    EventEndDateTime = start1001.AddHours(4),
                    Status = 3,
                    VenueRateAtBooking = 15000000m,
                    RentalUnitAtBooking = 0,
                    RentalQuantity = 4,
                    VenueCost = 60000000m,
                    AddOnCost = 0m,
                    TotalCost = 60000000m,
                    CreatedAt = new DateTime(2026, 9, 1)
                },
                new Booking
                {
                    ClientUserId = minhAnhId,
                    VenueId = gardenId,
                    EventTypeId = eventTypeId[eBirthday],
                    GuestCount = 80,
                    EventStartDateTime = start1002,
                    EventEndDateTime = start1002.AddHours(4),
                    Status = 1,
                    VenueRateAtBooking = 12000000m,
                    RentalUnitAtBooking = 0,
                    RentalQuantity = 4,
                    VenueCost = 48000000m,
                    AddOnCost = 0m,
                    TotalCost = 48000000m,
                    CreatedAt = new DateTime(2026, 10, 1)
                },
                new Booking
                {
                    ClientUserId = minhAnhId,
                    VenueId = skyId,
                    EventTypeId = eventTypeId[eGala],
                    GuestCount = 100,
                    EventStartDateTime = start1003,
                    EventEndDateTime = start1003.AddHours(4),
                    Status = 4,
                    VenueRateAtBooking = 2000000m,
                    RentalUnitAtBooking = 0,
                    RentalQuantity = 4,
                    VenueCost = 8000000m,
                    AddOnCost = 0m,
                    TotalCost = 8000000m,
                    CreatedAt = new DateTime(2026, 9, 20),
                    CancelledByUserId = coordinatorId,
                    CancelledAt = new DateTime(2026, 10, 5),
                    CancelReason = "Khách thay đổi kế hoạch tổ chức."
                },

                // Khách Hoàng Nam: Completed, Completed, Confirmed
                new Booking
                {
                    ClientUserId = hoangNamId,
                    VenueId = gardenId,
                    EventTypeId = eventTypeId[eBirthday],
                    GuestCount = 60,
                    EventStartDateTime = start1004,
                    EventEndDateTime = start1004.AddHours(4),
                    Status = 3,
                    VenueRateAtBooking = 12000000m,
                    RentalUnitAtBooking = 0,
                    RentalQuantity = 4,
                    VenueCost = 48000000m,
                    AddOnCost = 0m,
                    TotalCost = 48000000m,
                    CreatedAt = new DateTime(2026, 9, 5)
                },
                new Booking
                {
                    ClientUserId = hoangNamId,
                    VenueId = grandId,
                    EventTypeId = eventTypeId[eWedding],
                    GuestCount = 300,
                    EventStartDateTime = start1005,
                    EventEndDateTime = start1005.AddHours(4),
                    Status = 3,
                    VenueRateAtBooking = 15000000m,
                    RentalUnitAtBooking = 0,
                    RentalQuantity = 4,
                    VenueCost = 60000000m,
                    AddOnCost = 0m,
                    TotalCost = 60000000m,
                    CreatedAt = new DateTime(2026, 9, 10)
                },
                new Booking
                {
                    ClientUserId = hoangNamId,
                    VenueId = lotusId,
                    EventTypeId = eventTypeId[eConference],
                    GuestCount = 40,
                    EventStartDateTime = start1006,
                    EventEndDateTime = start1006.AddHours(4),
                    Status = 1,
                    VenueRateAtBooking = 800000m,
                    RentalUnitAtBooking = 0,
                    RentalQuantity = 4,
                    VenueCost = 3200000m,
                    AddOnCost = 0m,
                    TotalCost = 3200000m,
                    CreatedAt = new DateTime(2026, 10, 2)
                },

                // Khách Thu Hà: Completed, Confirmed
                new Booking
                {
                    ClientUserId = thuHaId,
                    VenueId = lotusId,
                    EventTypeId = eventTypeId[eConference],
                    GuestCount = 30,
                    EventStartDateTime = start1007,
                    EventEndDateTime = start1007.AddHours(4),
                    Status = 3,
                    VenueRateAtBooking = 800000m,
                    RentalUnitAtBooking = 0,
                    RentalQuantity = 4,
                    VenueCost = 3200000m,
                    AddOnCost = 0m,
                    TotalCost = 3200000m,
                    CreatedAt = new DateTime(2026, 10, 1)
                },
                new Booking
                {
                    ClientUserId = thuHaId,
                    VenueId = skyId,
                    EventTypeId = eventTypeId[eLaunch],
                    GuestCount = 100,
                    EventStartDateTime = start1008,
                    EventEndDateTime = start1008.AddHours(4),
                    Status = 1,
                    VenueRateAtBooking = 2000000m,
                    RentalUnitAtBooking = 0,
                    RentalQuantity = 4,
                    VenueCost = 8000000m,
                    AddOnCost = 0m,
                    TotalCost = 8000000m,
                    CreatedAt = new DateTime(2026, 10, 3)
                }
            );
            context.SaveChanges();

            // Tra Id thật của các booking vừa seed (DB tự cấp, không phải 1001...1008)
            Func<int, int, DateTime, int> bookingId = (client, venue, start) =>
                context.Bookings
                    .Where(b => b.ClientUserId == client && b.VenueId == venue && b.EventStartDateTime == start)
                    .Select(b => b.BookingId)
                    .First();

            int bk1001 = bookingId(minhAnhId, grandId, start1001);
            int bk1002 = bookingId(minhAnhId, gardenId, start1002);
            // Booking 1003 đã Cancelled nên cố ý không có Payment
            int bk1004 = bookingId(hoangNamId, gardenId, start1004);
            int bk1005 = bookingId(hoangNamId, grandId, start1005);
            int bk1006 = bookingId(hoangNamId, lotusId, start1006);
            int bk1007 = bookingId(thuHaId, lotusId, start1007);
            int bk1008 = bookingId(thuHaId, skyId, start1008);

            // ==================== DEMO PAYMENTS ====================
            // Tất cả Completed. Chia 30% đặt cọc (Deposit) + 70% thanh toán cuối (FinalPayment).
            // Khóa tìm dòng: (BookingId, PaymentType) - mỗi booking tối đa 1 cọc + 1 thanh toán cuối.
            // RecordedByUserId = coordinatorId.
            context.Payments.AddOrUpdate(p => new { p.BookingId, p.PaymentType },

                // TotalCost = 60,000,000
                new Payment { BookingId = bk1001, Amount = 18000000m, PaymentType = 0, PaymentMethod = "Bank Transfer", PaymentStatus = 1, PaymentDate = new DateTime(2026, 9, 2), RecordedByUserId = coordinatorId },
                new Payment { BookingId = bk1001, Amount = 42000000m, PaymentType = 1, PaymentMethod = "Bank Transfer", PaymentStatus = 1, PaymentDate = new DateTime(2026, 9, 10), RecordedByUserId = coordinatorId },

                // TotalCost = 48,000,000
                new Payment { BookingId = bk1002, Amount = 14400000m, PaymentType = 0, PaymentMethod = "Bank Transfer", PaymentStatus = 1, PaymentDate = new DateTime(2026, 10, 2), RecordedByUserId = coordinatorId },
                new Payment { BookingId = bk1002, Amount = 33600000m, PaymentType = 1, PaymentMethod = "Bank Transfer", PaymentStatus = 1, PaymentDate = new DateTime(2026, 10, 3), RecordedByUserId = coordinatorId },

                // TotalCost = 48,000,000
                new Payment { BookingId = bk1004, Amount = 14400000m, PaymentType = 0, PaymentMethod = "Credit Card", PaymentStatus = 1, PaymentDate = new DateTime(2026, 9, 6), RecordedByUserId = coordinatorId },
                new Payment { BookingId = bk1004, Amount = 33600000m, PaymentType = 1, PaymentMethod = "Credit Card", PaymentStatus = 1, PaymentDate = new DateTime(2026, 9, 20), RecordedByUserId = coordinatorId },

                // TotalCost = 60,000,000
                new Payment { BookingId = bk1005, Amount = 18000000m, PaymentType = 0, PaymentMethod = "Bank Transfer", PaymentStatus = 1, PaymentDate = new DateTime(2026, 9, 11), RecordedByUserId = coordinatorId },
                new Payment { BookingId = bk1005, Amount = 42000000m, PaymentType = 1, PaymentMethod = "Bank Transfer", PaymentStatus = 1, PaymentDate = new DateTime(2026, 9, 25), RecordedByUserId = coordinatorId },

                // TotalCost = 3,200,000
                new Payment { BookingId = bk1006, Amount = 960000m, PaymentType = 0, PaymentMethod = "Credit Card", PaymentStatus = 1, PaymentDate = new DateTime(2026, 10, 3), RecordedByUserId = coordinatorId },
                new Payment { BookingId = bk1006, Amount = 2240000m, PaymentType = 1, PaymentMethod = "Credit Card", PaymentStatus = 1, PaymentDate = new DateTime(2026, 10, 4), RecordedByUserId = coordinatorId },

                // TotalCost = 3,200,000
                new Payment { BookingId = bk1007, Amount = 960000m, PaymentType = 0, PaymentMethod = "Bank Transfer", PaymentStatus = 1, PaymentDate = new DateTime(2026, 10, 2), RecordedByUserId = coordinatorId },
                new Payment { BookingId = bk1007, Amount = 2240000m, PaymentType = 1, PaymentMethod = "Bank Transfer", PaymentStatus = 1, PaymentDate = new DateTime(2026, 10, 12), RecordedByUserId = coordinatorId },

                // TotalCost = 8,000,000
                new Payment { BookingId = bk1008, Amount = 2400000m, PaymentType = 0, PaymentMethod = "Credit Card", PaymentStatus = 1, PaymentDate = new DateTime(2026, 10, 4), RecordedByUserId = coordinatorId },
                new Payment { BookingId = bk1008, Amount = 5600000m, PaymentType = 1, PaymentMethod = "Credit Card", PaymentStatus = 1, PaymentDate = new DateTime(2026, 10, 5), RecordedByUserId = coordinatorId }
            );
            context.SaveChanges();

            // ==================== DEMO FEEDBACK ====================
            // Chỉ booking Completed mới có Feedback. Khóa tìm dòng: BookingId (UNIQUE).
            context.Feedbacks.AddOrUpdate(f => f.BookingId,
                new Feedback { BookingId = bk1001, Rating = 5, Comment = "Không gian đẹp, nhân viên hỗ trợ nhiệt tình.", CreatedAt = new DateTime(2026, 9, 21) },
                new Feedback { BookingId = bk1004, Rating = 4, Comment = "Địa điểm thuận tiện, buổi tiệc diễn ra tốt.", CreatedAt = new DateTime(2026, 10, 4) },
                new Feedback { BookingId = bk1005, Rating = 5, Comment = "Sảnh rộng rãi, trang thiết bị đáp ứng tốt nhu cầu.", CreatedAt = new DateTime(2026, 10, 11) },
                new Feedback { BookingId = bk1007, Rating = 3, Comment = "Địa điểm ổn, có thể cải thiện thêm khâu chuẩn bị.", CreatedAt = new DateTime(2026, 10, 19) }
            );
            context.SaveChanges();
        }
    }
}