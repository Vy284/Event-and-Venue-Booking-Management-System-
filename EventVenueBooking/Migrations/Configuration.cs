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
                    PasswordHash = "admin123",
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
                    PasswordHash = "user123",
                    Phone = "0909876543",
                    Role = 1, // Receptionist
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );

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
                new Venue { VenueId = 1, Name = "Sảnh Grand Ballroom", VenueTypeId = 1, Capacity = 500, Description = "Sảnh tiệc sang trọng trần cao.", RentalRate = 15000000, RentalUnit = 1, Location = "Tầng 1 - Khu A", Status = 0, CreatedAt = DateTime.Now, CreatedByUserId = 1 },
                new Venue { VenueId = 2, Name = "Sân Vườn Garden Eden", VenueTypeId = 2, Capacity = 300, Description = "Không gian sân vườn thoáng đãng.", RentalRate = 12000000, RentalUnit = 1, Location = "Khu Sân Vườn", Status = 0, CreatedAt = DateTime.Now, CreatedByUserId = 1 },
                new Venue { VenueId = 3, Name = "Rooftop Sky Lounge", VenueTypeId = 3, Capacity = 150, Description = "Tầng thượng view thành phố.", RentalRate = 2000000, RentalUnit = 0, Location = "Tầng 12", Status = 0, CreatedAt = DateTime.Now, CreatedByUserId = 1 },
                new Venue { VenueId = 4, Name = "Phòng Hội Nghị Lotus", VenueTypeId = 4, Capacity = 100, Description = "Phòng họp trang bị hiện đại.", RentalRate = 800000, RentalUnit = 0, Location = "Tầng 2", Status = 0, CreatedAt = DateTime.Now, CreatedByUserId = 1 }
            );

            context.SaveChanges();

            //Temporary images for venues, có thể thay thế lại sau nếu cần
            context.VenueImages.AddOrUpdate(img => img.ImageId,
                new VenueImage { ImageId = 1, VenueId = 1, ImageUrl = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800", IsPrimary = true },
                new VenueImage { ImageId = 2, VenueId = 2, ImageUrl = "https://images.unsplash.com/photo-1587271407850-8d438ca9fdf2?w=800", IsPrimary = true },
                new VenueImage { ImageId = 3, VenueId = 3, ImageUrl = "https://images.unsplash.com/photo-1533105079780-92b9be482077?w=800", IsPrimary = true },
                new VenueImage { ImageId = 4, VenueId = 4, ImageUrl = "https://images.unsplash.com/photo-1431540015161-0bf868a2d407?w=800", IsPrimary = true }
            );
        }
    }
}
