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

        protected override void Seed(EventVenueBooking.Database.ApplicationDbContext context)
        {
            context.Users.AddOrUpdate(u => u.UserId,
                new User
                {
                    UserId = 1,
                    FullName = "Quản Lý Hệ Thống",
                    Email = "admin@eventvenue.com",
                    PasswordHash = "$2a$11$0VIDReRhGnqWLo6SWTBKI.y8Y8dDKt1ba1sOgv7quUmI9L2352SFm", //BCrypt.Net.BCrypt.HashPassword("admin123")
                    Phone = "0901234567",
                    Role = 2, // Manager / Admin
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    UserId = 2,
                    FullName = "Nhân Viên Lễ Tân",
                    Email = "receptionist@eventvenue.com",
                    PasswordHash = "$2a$11$tZVy9U9SCAIyrkJl7o194.9KrwMZ7k3IAlFD8QiwO6pXruJHQMPx2", //BCrypt.Net.BCrypt.HashPassword("user123")
                    Phone = "0909876543",
                    Role = 1, // Receptionist
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );

            context.SaveChanges();

            // Seed demo customers by Email to avoid duplicate-email errors.

            SeedCustomer(context, new User
            {
                UserId = 1001,
                FullName = "Nguyễn Minh Anh",
                Email = "minhanh@example.com",
                PasswordHash = "$2a$11$tZVy9U9SCAIyrkJl7o194.9KrwMZ7k3IAlFD8QiwO6pXruJHQMPx2",
                Phone = "0911000001",
                Role = 0,
                IsActive = true,
                CreatedAt = new DateTime(2026, 9, 1)
            });

            SeedCustomer(context, new User
            {
                UserId = 1002,
                FullName = "Trần Hoàng Nam",
                Email = "hoangnam@example.com",
                PasswordHash = "$2a$11$tZVy9U9SCAIyrkJl7o194.9KrwMZ7k3IAlFD8QiwO6pXruJHQMPx2",
                Phone = "0911000002",
                Role = 0,
                IsActive = true,
                CreatedAt = new DateTime(2026, 9, 2)
            });

            SeedCustomer(context, new User
            {
                UserId = 1003,
                FullName = "Lê Thu Hà",
                Email = "thuha@example.com",
                PasswordHash = "$2a$11$tZVy9U9SCAIyrkJl7o194.9KrwZ7k3IAlFD8QiwO6pXruJHQMPx2",
                Phone = "0911000003",
                Role = 0,
                IsActive = true,
                CreatedAt = new DateTime(2026, 9, 3)
            });

            context.SaveChanges();

            context.VenueTypes.AddOrUpdate(vt => vt.VenueTypeId,
                new VenueType { VenueTypeId = 1, TypeName = "Sảnh Trong Nhà (Indoor Ballroom)", IsActive = true },
                new VenueType { VenueTypeId = 2, TypeName = "Sân Vườn (Outdoor Garden)", IsActive = true },
                new VenueType { VenueTypeId = 3, TypeName = "Rooftop / Tầng Thượng", IsActive = true },
                new VenueType { VenueTypeId = 4, TypeName = "Phòng Hội Nghị (Conference Hall)", IsActive = true }
            );

            context.Facilities.AddOrUpdate(f => f.FacilityId,
                new Facility { FacilityId = 1, FacilityName = "Màn hình LED 4K cỡ lớn", IsActive = true },
                new Facility { FacilityId = 2, FacilityName = "Hệ thống Âm thanh / Ánh sáng", IsActive = true },
                new Facility { FacilityId = 3, FacilityName = "Sân khấu di động", IsActive = true },
                new Facility { FacilityId = 4, FacilityName = "Máy chiếu & Màn chiếu HD", IsActive = true },
                new Facility { FacilityId = 5, FacilityName = "Hệ thống Điều hòa công suất lớn", IsActive = true },
                new Facility { FacilityId = 6, FacilityName = "Wifi tốc độ cao", IsActive = true },
                new Facility { FacilityId = 7, FacilityName = "Bãi đỗ xe ô tô / xe máy", IsActive = true }
            );

            context.EventTypes.AddOrUpdate(et => et.EventTypeId,
                new EventType { EventTypeId = 1, TypeName = "Tiệc Cưới (Wedding)", IsActive = true },
                new EventType { EventTypeId = 2, TypeName = "Hội Thảo / Conference", IsActive = true },
                new EventType { EventTypeId = 3, TypeName = "Tiệc Sinh Nhật / Anniversary", IsActive = true },
                new EventType { EventTypeId = 4, TypeName = "Sự Kiện Doanh Nghiệp / Gala Dinner", IsActive = true },
                new EventType { EventTypeId = 5, TypeName = "Lễ Ra Mắt Sản Phẩm", IsActive = true }
            );

            context.AddOnServices.AddOrUpdate(a => a.AddOnId,
                new AddOnService { AddOnId = 1, Name = "MC Dẫn Chương Trình", Description = "MC chuyên nghiệp dẫn tiệc cưới, sự kiện", Price = 2000000, Category = "Nhân sự", IsActive = true },
                new AddOnService { AddOnId = 2, Name = "Trang Trí Hoa Tươi", Description = "Trang trí bàn tiệc và cổng chào theo yêu cầu", Price = 5000000, Category = "Trang trí", IsActive = true },
                new AddOnService { AddOnId = 3, Name = "Ban Nhạc Acoustic", Description = "Ban nhạc biểu diễn giao lưu trong buổi tiệc", Price = 3500000, Category = "Giải trí", IsActive = true },
                new AddOnService { AddOnId = 4, Name = "Chụp Ảnh / Quay Phim", Description = "Thợ chụp và quay phim chuyên nghiệp", Price = 4000000, Category = "Truyền thông", IsActive = true }
            );

            context.SaveChanges();

            context.Venues.AddOrUpdate(v => v.VenueId,
                new Venue { VenueId = 1, Name = "Sảnh Grand Ballroom", VenueTypeId = 1, Capacity = 500, Description = "Sảnh tiệc sang trọng trần cao.", RentalRate = 15000000, RentalUnit = 0, Location = "Tầng 1 - Khu A", Status = 1, CreatedAt = DateTime.Now, CreatedByUserId = 1 },
                new Venue { VenueId = 2, Name = "Sân Vườn Garden Eden", VenueTypeId = 2, Capacity = 300, Description = "Không gian sân vườn thoáng đãng.", RentalRate = 12000000, RentalUnit = 0, Location = "Khu Sân Vườn", Status = 1, CreatedAt = DateTime.Now, CreatedByUserId = 1 },
                new Venue { VenueId = 3, Name = "Rooftop Sky Lounge", VenueTypeId = 3, Capacity = 150, Description = "Tầng thượng view thành phố.", RentalRate = 2000000, RentalUnit = 0, Location = "Tầng 12", Status = 1, CreatedAt = DateTime.Now, CreatedByUserId = 1 },
                new Venue { VenueId = 4, Name = "Phòng Hội Nghị Lotus", VenueTypeId = 4, Capacity = 100, Description = "Phòng họp trang bị hiện đại.", RentalRate = 800000, RentalUnit = 0, Location = "Tầng 2", Status = 1, CreatedAt = DateTime.Now, CreatedByUserId = 1 }
            );

            context.SaveChanges();
   
            // ==================== DEMO VENUE IMAGES ====================
            // Four images per venue; exactly one primary image per venue.
            // Temporary image URLs can be replaced later.

            context.VenueImages.AddOrUpdate(img => img.ImageId,

                // Venue 1: Grand Ballroom
                new VenueImage
                {
                    ImageId = 1,
                    VenueId = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800",
                    IsPrimary = true
                },
                new VenueImage
                {
                    ImageId = 5,
                    VenueId = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865?w=800",
                    IsPrimary = false
                },
                new VenueImage
                {
                    ImageId = 6,
                    VenueId = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1507504031003-b417219a0fde?w=800",
                    IsPrimary = false
                },
                new VenueImage
                {
                    ImageId = 7,
                    VenueId = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800",
                    IsPrimary = false
                },

                // Venue 2: Garden Eden
                new VenueImage
                {
                    ImageId = 2,
                    VenueId = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1587271407850-8d438ca9fdf2?w=800",
                    IsPrimary = true
                },
                new VenueImage
                {
                    ImageId = 8,
                    VenueId = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800",
                    IsPrimary = false
                },
                new VenueImage
                {
                    ImageId = 9,
                    VenueId = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800",
                    IsPrimary = false
                },
                new VenueImage
                {
                    ImageId = 10,
                    VenueId = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1507504031003-b417219a0fde?w=800",
                    IsPrimary = false
                },

                // Venue 3: Sky Lounge
                new VenueImage
                {
                    ImageId = 3,
                    VenueId = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1533105079780-92b9be482077?w=800",
                    IsPrimary = true
                },
                new VenueImage
                {
                    ImageId = 11,
                    VenueId = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865?w=800",
                    IsPrimary = false
                },
                new VenueImage
                {
                    ImageId = 12,
                    VenueId = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800",
                    IsPrimary = false
                },
                new VenueImage
                {
                    ImageId = 13,
                    VenueId = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800",
                    IsPrimary = false
                },

                // Venue 4: Lotus Conference Hall
                new VenueImage
                {
                    ImageId = 4,
                    VenueId = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1431540015161-0bf868a2d407?w=800",
                    IsPrimary = true
                },
                new VenueImage
                {
                    ImageId = 14,
                    VenueId = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865?w=800",
                    IsPrimary = false
                },
                new VenueImage
                {
                    ImageId = 15,
                    VenueId = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1507504031003-b417219a0fde?w=800",
                    IsPrimary = false
                },
                new VenueImage
                {
                    ImageId = 16,
                    VenueId = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800",
                    IsPrimary = false
                }
            );

            context.SaveChanges();


            // ==================== DEMO BOOKINGS ====================
            // Status: 0 = Pending, 1 = Confirmed, 2 = InProgress,
            //         3 = Completed, 4 = Cancelled
            //
            // RentalUnitAtBooking = 0 means hourly rental.
            // All demo bookings last 4 hours, so RentalQuantity = 4.
            // VenueCost = VenueRateAtBooking * RentalQuantity.

            //context.Bookings.AddOrUpdate(b => b.BookingId,

            //    // Customer 1001: Completed, Confirmed, Cancelled
            //    new Booking
            //    {
            //        BookingId = 1001,
            //        ClientUserId = 1001,
            //        VenueId = 1,
            //        EventTypeId = 1,
            //        GuestCount = 200,
            //        EventStartDateTime = new DateTime(2026, 9, 20, 18, 0, 0),
            //        EventEndDateTime = new DateTime(2026, 9, 20, 22, 0, 0),
            //        Status = 3,
            //        VenueRateAtBooking = 15000000m,
            //        RentalUnitAtBooking = 0,
            //        RentalQuantity = 4,
            //        VenueCost = 60000000m,
            //        AddOnCost = 0m,
            //        TotalCost = 60000000m,
            //        CreatedAt = new DateTime(2026, 9, 1)
            //    },
            //    new Booking
            //    {
            //        BookingId = 1002,
            //        ClientUserId = 1001,
            //        VenueId = 2,
            //        EventTypeId = 3,
            //        GuestCount = 80,
            //        EventStartDateTime = new DateTime(2026, 11, 15, 17, 0, 0),
            //        EventEndDateTime = new DateTime(2026, 11, 15, 21, 0, 0),
            //        Status = 1,
            //        VenueRateAtBooking = 12000000m,
            //        RentalUnitAtBooking = 0,
            //        RentalQuantity = 4,
            //        VenueCost = 48000000m,
            //        AddOnCost = 0m,
            //        TotalCost = 48000000m,
            //        CreatedAt = new DateTime(2026, 10, 1)
            //    },
            //    new Booking
            //    {
            //        BookingId = 1003,
            //        ClientUserId = 1001,
            //        VenueId = 3,
            //        EventTypeId = 4,
            //        GuestCount = 100,
            //        EventStartDateTime = new DateTime(2026, 10, 10, 18, 0, 0),
            //        EventEndDateTime = new DateTime(2026, 10, 10, 22, 0, 0),
            //        Status = 4,
            //        VenueRateAtBooking = 2000000m,
            //        RentalUnitAtBooking = 0,
            //        RentalQuantity = 4,
            //        VenueCost = 8000000m,
            //        AddOnCost = 0m,
            //        TotalCost = 8000000m,
            //        CreatedAt = new DateTime(2026, 9, 20),
            //        CancelledByUserId = 2,
            //        CancelledAt = new DateTime(2026, 10, 5),
            //        CancelReason = "Khách thay đổi kế hoạch tổ chức."
            //    },

            //    // Customer 1002: Completed, Completed, Confirmed
            //    new Booking
            //    {
            //        BookingId = 1004,
            //        ClientUserId = 1002,
            //        VenueId = 2,
            //        EventTypeId = 3,
            //        GuestCount = 60,
            //        EventStartDateTime = new DateTime(2026, 10, 3, 17, 0, 0),
            //        EventEndDateTime = new DateTime(2026, 10, 3, 21, 0, 0),
            //        Status = 3,
            //        VenueRateAtBooking = 12000000m,
            //        RentalUnitAtBooking = 0,
            //        RentalQuantity = 4,
            //        VenueCost = 48000000m,
            //        AddOnCost = 0m,
            //        TotalCost = 48000000m,
            //        CreatedAt = new DateTime(2026, 9, 5)
            //    },
            //    new Booking
            //    {
            //        BookingId = 1005,
            //        ClientUserId = 1002,
            //        VenueId = 1,
            //        EventTypeId = 1,
            //        GuestCount = 300,
            //        EventStartDateTime = new DateTime(2026, 10, 10, 17, 0, 0),
            //        EventEndDateTime = new DateTime(2026, 10, 10, 21, 0, 0),
            //        Status = 3,
            //        VenueRateAtBooking = 15000000m,
            //        RentalUnitAtBooking = 0,
            //        RentalQuantity = 4,
            //        VenueCost = 60000000m,
            //        AddOnCost = 0m,
            //        TotalCost = 60000000m,
            //        CreatedAt = new DateTime(2026, 9, 10)
            //    },
            //    new Booking
            //    {
            //        BookingId = 1006,
            //        ClientUserId = 1002,
            //        VenueId = 4,
            //        EventTypeId = 2,
            //        GuestCount = 40,
            //        EventStartDateTime = new DateTime(2026, 11, 20, 8, 0, 0),
            //        EventEndDateTime = new DateTime(2026, 11, 20, 12, 0, 0),
            //        Status = 1,
            //        VenueRateAtBooking = 800000m,
            //        RentalUnitAtBooking = 0,
            //        RentalQuantity = 4,
            //        VenueCost = 3200000m,
            //        AddOnCost = 0m,
            //        TotalCost = 3200000m,
            //        CreatedAt = new DateTime(2026, 10, 2)
            //    },

            //    // Customer 1003: Completed, Confirmed
            //    new Booking
            //    {
            //        BookingId = 1007,
            //        ClientUserId = 1003,
            //        VenueId = 4,
            //        EventTypeId = 2,
            //        GuestCount = 30,
            //        EventStartDateTime = new DateTime(2026, 10, 18, 8, 0, 0),
            //        EventEndDateTime = new DateTime(2026, 10, 18, 12, 0, 0),
            //        Status = 3,
            //        VenueRateAtBooking = 800000m,
            //        RentalUnitAtBooking = 0,
            //        RentalQuantity = 4,
            //        VenueCost = 3200000m,
            //        AddOnCost = 0m,
            //        TotalCost = 3200000m,
            //        CreatedAt = new DateTime(2026, 10, 1)
            //    },
            //    new Booking
            //    {
            //        BookingId = 1008,
            //        ClientUserId = 1003,
            //        VenueId = 3,
            //        EventTypeId = 5,
            //        GuestCount = 100,
            //        EventStartDateTime = new DateTime(2026, 11, 25, 18, 0, 0),
            //        EventEndDateTime = new DateTime(2026, 11, 25, 22, 0, 0),
            //        Status = 1,
            //        VenueRateAtBooking = 2000000m,
            //        RentalUnitAtBooking = 0,
            //        RentalQuantity = 4,
            //        VenueCost = 8000000m,
            //        AddOnCost = 0m,
            //        TotalCost = 8000000m,
            //        CreatedAt = new DateTime(2026, 10, 3)
            //    }
            //);

            //context.SaveChanges();




            //// ==================== DEMO PAYMENTS ====================
            //// All seeded payments are Completed.
            //// Booking 1003 is Cancelled and intentionally has no Payment.
            //// Demo split: 30% Deposit + 70% FinalPayment.
            //// Amounts are recalculated according to the updated booking totals.
            //// RecordedByUserId = 2 (existing coordinator account).

            //context.Payments.AddOrUpdate(p => p.PaymentId,

            //    // Booking 1001: TotalCost = 60,000,000
            //    new Payment
            //    {
            //        PaymentId = 1001,
            //        BookingId = 1001,
            //        Amount = 18000000m,
            //        PaymentType = 0,
            //        PaymentMethod = "Bank Transfer",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 9, 2),
            //        RecordedByUserId = 2
            //    },
            //    new Payment
            //    {
            //        PaymentId = 1002,
            //        BookingId = 1001,
            //        Amount = 42000000m,
            //        PaymentType = 1,
            //        PaymentMethod = "Bank Transfer",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 9, 10),
            //        RecordedByUserId = 2
            //    },

            //    // Booking 1002: TotalCost = 48,000,000
            //    new Payment
            //    {
            //        PaymentId = 1003,
            //        BookingId = 1002,
            //        Amount = 14400000m,
            //        PaymentType = 0,
            //        PaymentMethod = "Bank Transfer",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 10, 2),
            //        RecordedByUserId = 2
            //    },
            //    new Payment
            //    {
            //        PaymentId = 1004,
            //        BookingId = 1002,
            //        Amount = 33600000m,
            //        PaymentType = 1,
            //        PaymentMethod = "Bank Transfer",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 10, 3),
            //        RecordedByUserId = 2
            //    },

            //    // Booking 1004: TotalCost = 48,000,000
            //    new Payment
            //    {
            //        PaymentId = 1005,
            //        BookingId = 1004,
            //        Amount = 14400000m,
            //        PaymentType = 0,
            //        PaymentMethod = "Credit Card",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 9, 6),
            //        RecordedByUserId = 2
            //    },
            //    new Payment
            //    {
            //        PaymentId = 1006,
            //        BookingId = 1004,
            //        Amount = 33600000m,
            //        PaymentType = 1,
            //        PaymentMethod = "Credit Card",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 9, 20),
            //        RecordedByUserId = 2
            //    },

            //    // Booking 1005: TotalCost = 60,000,000
            //    new Payment
            //    {
            //        PaymentId = 1007,
            //        BookingId = 1005,
            //        Amount = 18000000m,
            //        PaymentType = 0,
            //        PaymentMethod = "Bank Transfer",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 9, 11),
            //        RecordedByUserId = 2
            //    },
            //    new Payment
            //    {
            //        PaymentId = 1008,
            //        BookingId = 1005,
            //        Amount = 42000000m,
            //        PaymentType = 1,
            //        PaymentMethod = "Bank Transfer",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 9, 25),
            //        RecordedByUserId = 2
            //    },

            //    // Booking 1006: TotalCost = 3,200,000
            //    new Payment
            //    {
            //        PaymentId = 1009,
            //        BookingId = 1006,
            //        Amount = 960000m,
            //        PaymentType = 0,
            //        PaymentMethod = "Credit Card",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 10, 3),
            //        RecordedByUserId = 2
            //    },
            //    new Payment
            //    {
            //        PaymentId = 1010,
            //        BookingId = 1006,
            //        Amount = 2240000m,
            //        PaymentType = 1,
            //        PaymentMethod = "Credit Card",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 10, 4),
            //        RecordedByUserId = 2
            //    },

            //    // Booking 1007: TotalCost = 3,200,000
            //    new Payment
            //    {
            //        PaymentId = 1011,
            //        BookingId = 1007,
            //        Amount = 960000m,
            //        PaymentType = 0,
            //        PaymentMethod = "Bank Transfer",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 10, 2),
            //        RecordedByUserId = 2
            //    },
            //    new Payment
            //    {
            //        PaymentId = 1012,
            //        BookingId = 1007,
            //        Amount = 2240000m,
            //        PaymentType = 1,
            //        PaymentMethod = "Bank Transfer",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 10, 12),
            //        RecordedByUserId = 2
            //    },

            //    // Booking 1008: TotalCost = 8,000,000
            //    new Payment
            //    {
            //        PaymentId = 1013,
            //        BookingId = 1008,
            //        Amount = 2400000m,
            //        PaymentType = 0,
            //        PaymentMethod = "Credit Card",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 10, 4),
            //        RecordedByUserId = 2
            //    },
            //    new Payment
            //    {
            //        PaymentId = 1014,
            //        BookingId = 1008,
            //        Amount = 5600000m,
            //        PaymentType = 1,
            //        PaymentMethod = "Credit Card",
            //        PaymentStatus = 1,
            //        PaymentDate = new DateTime(2026, 10, 5),
            //        RecordedByUserId = 2
            //    }
            //);

            //context.SaveChanges();



            // ==================== DEMO FEEDBACK ====================
            // Only completed bookings have Feedback.

        //    context.Feedbacks.AddOrUpdate(f => f.FeedbackId,
        //        new Feedback
        //        {
        //            FeedbackId = 1001,
        //            BookingId = 1001,
        //            Rating = 5,
        //            Comment = "Không gian đẹp, nhân viên hỗ trợ nhiệt tình.",
        //            CreatedAt = new DateTime(2026, 9, 21)
        //        },
        //        new Feedback
        //        {
        //            FeedbackId = 1002,
        //            BookingId = 1004,
        //            Rating = 4,
        //            Comment = "Địa điểm thuận tiện, buổi tiệc diễn ra tốt.",
        //            CreatedAt = new DateTime(2026, 10, 4)
        //        },
        //        new Feedback
        //        {
        //            FeedbackId = 1003,
        //            BookingId = 1005,
        //            Rating = 5,
        //            Comment = "Sảnh rộng rãi, trang thiết bị đáp ứng tốt nhu cầu.",
        //            CreatedAt = new DateTime(2026, 10, 11)
        //        },
        //        new Feedback
        //        {
        //            FeedbackId = 1004,
        //            BookingId = 1007,
        //            Rating = 3,
        //            Comment = "Địa điểm ổn, có thể cải thiện thêm khâu chuẩn bị.",
        //            CreatedAt = new DateTime(2026, 10, 19)
        //        }
        //    );

        //    context.SaveChanges();
        }

        private static void SeedCustomer(
    EventVenueBooking.Database.ApplicationDbContext context,
    User customer)
        {
            var existingCustomer = context.Users
                .SingleOrDefault(u => u.Email == customer.Email);

            if (existingCustomer != null)
            {
                // Keep the existing UserId and CreatedAt.
                // Do not overwrite the existing customer account.
                return;
            }

            var existingId = context.Users.Find(customer.UserId);

            if (existingId != null)
            {
                throw new InvalidOperationException(
                    "Cannot seed customer '" + customer.Email
                    + "' because UserId " + customer.UserId
                    + " already belongs to " + existingId.Email);
            }

            context.Users.Add(customer);
        }
    }
}
