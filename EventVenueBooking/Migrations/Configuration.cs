namespace EventVenueBooking.Migrations
{
    using EventVenueBooking.Entities;
    using EventVenueBooking.Services;
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

            // Ảnh chính của từng sảnh (giữ nguyên ảnh chính IsPrimary = true)
            context.VenueImages.AddOrUpdate(img => img.ImageId,
                new VenueImage { ImageId = 1, VenueId = 1, ImageUrl = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800", IsPrimary = true },
                new VenueImage { ImageId = 2, VenueId = 2, ImageUrl = "https://images.unsplash.com/photo-1587271407850-8d438ca9fdf2?w=800", IsPrimary = true },
                new VenueImage { ImageId = 3, VenueId = 3, ImageUrl = "https://images.unsplash.com/photo-1533105079780-92b9be482077?w=800", IsPrimary = true },
                new VenueImage { ImageId = 4, VenueId = 4, ImageUrl = "https://images.unsplash.com/photo-1431540015161-0bf868a2d407?w=800", IsPrimary = true }
            );

            context.SaveChanges();

            // Nạp bộ 3 ảnh phụ mới theo đúng chủ đề, đồng thời dọn sạch ảnh phụ lỗi/cũ
            SeedVenueGallery(context);

            // Feedback mẫu
            SeedSampleFeedback(context);
        }

        // Tạo link ảnh Pexels từ id ảnh
        private static string PexelsUrl(int photoId)
        {
            return "https://images.pexels.com/photos/" + photoId + "/pexels-photo-" + photoId + ".jpeg?auto=compress&cs=tinysrgb&w=800";
        }

        // Dọn dẹp ảnh phụ cũ và nạp bộ ảnh mới đúng chủ đề cho từng sảnh
        private void SeedVenueGallery(EventVenueBooking.Database.ApplicationDbContext context)
        {
            // Tự động xóa bỏ các ảnh phụ cũ (IsPrimary = false) thuộc về ảnh mẫu Unsplash/Picsum để tránh bị lặp
            var duplicateImages = context.VenueImages
                .Where(i => !i.IsPrimary && (i.ImageUrl.Contains("unsplash.com") || i.ImageUrl.Contains("picsum.photos")))
                .ToList();

            if (duplicateImages.Any())
            {
                context.VenueImages.RemoveRange(duplicateImages);
                context.SaveChanges();
            }

            // Danh sách 3 ảnh chuẩn Pexels cho từng sảnh (1 - 4)
            string[][] galleryByVenue =
            {
                // 1 - Sảnh Grand Ballroom
                new[] { PexelsUrl(30311728), PexelsUrl(33852468), PexelsUrl(17023014) },
                // 2 - Sân Vườn Garden Eden
                new[] { PexelsUrl(27132464), PexelsUrl(37828118), PexelsUrl(35985252) },
                // 3 - Rooftop Sky Lounge
                new[] { PexelsUrl(37836009), PexelsUrl(40041417), PexelsUrl(33389223) },
                // 4 - Phòng Hội Nghị Lotus
                new[] { PexelsUrl(16859956), PexelsUrl(33362590), PexelsUrl(36900079) }
            };

            for (int v = 0; v < galleryByVenue.Length; v++)
            {
                int venueId = v + 1;

                foreach (string url in galleryByVenue[v])
                {
                    if (context.VenueImages.Any(i => i.VenueId == venueId && i.ImageUrl == url)) continue;

                    context.VenueImages.Add(new VenueImage
                    {
                        VenueId = venueId,
                        ImageUrl = url,
                        IsPrimary = false
                    });
                }
            }

            context.SaveChanges();
        }

        // 3 khách mẫu + 8 booking đã hoàn tất + mỗi booking 1 feedback
        private void SeedSampleFeedback(EventVenueBooking.Database.ApplicationDbContext context)
        {
            if (context.Feedbacks.Any()) return;   // đã có feedback thì không seed lại

            var staff = context.Users.FirstOrDefault(u => u.Role == 2);
            if (staff == null) return;

            string[] clientNames = { "Nguyễn Minh Anh", "Trần Quốc Bảo", "Lê Thu Hà" };
            string[] clientEmails = { "minhanh@example.com", "quocbao@example.com", "thuha@example.com" };
            string[] clientPhones = { "0912345601", "0912345602", "0912345603" };

            string passwordHash = null;
            for (int c = 0; c < clientEmails.Length; c++)
            {
                string email = clientEmails[c];
                if (context.Users.Any(u => u.Email == email)) continue;

                if (passwordHash == null)
                    passwordHash = BCrypt.Net.BCrypt.HashPassword("Client@123");

                context.Users.Add(new User
                {
                    FullName = clientNames[c],
                    Email = email,
                    Phone = clientPhones[c],
                    PasswordHash = passwordHash,
                    Role = 0,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }
            context.SaveChanges();

            var clients = context.Users
                .Where(u => clientEmails.Contains(u.Email))
                .OrderBy(u => u.UserId)
                .ToList();
            if (clients.Count == 0) return;

            int[] venueIds = { 1, 2, 3, 4, 1, 3, 2, 4 };
            int[] eventTypeIds = { 1, 1, 3, 2, 4, 4, 3, 5 };
            int[] addOnIds = { 2, 4, 3, 0, 1, 0, 2, 0 };
            int[] ratings = { 5, 5, 4, 5, 4, 3, 5, 4 };
            string[] comments =
            {
                "Sảnh rộng rãi, trần cao, trang trí hoa rất đẹp. Nhân viên hỗ trợ nhiệt tình, gia đình mình rất hài lòng!",
                "Không gian sân vườn thoáng, ảnh cưới lên rất đẹp. Ban tổ chức phối hợp nhịp nhàng suốt buổi tiệc.",
                "View thành phố về đêm tuyệt vời, bạn bè ai cũng thích. Chỉ tiếc thang máy hơi đông vào giờ cao điểm.",
                "Phòng họp trang bị hiện đại, máy chiếu và âm thanh hoạt động tốt. Hội thảo diễn ra suôn sẻ.",
                "Không gian sang trọng, âm thanh ánh sáng tốt. Sẽ quay lại tổ chức sự kiện lần sau.",
                "Nhìn chung ổn nhưng gió khá lớn vào buổi tối, nên có thêm phương án che chắn.",
                "Tuyệt vời! Khách mời ai cũng khen không gian và cách phục vụ.",
                "Quy trình đặt phòng và thanh toán rõ ràng, minh bạch. Rất đáng tiền."
            };

            for (int i = 0; i < comments.Length; i++)
            {
                var venue = context.Venues.Find(venueIds[i]);
                var eventType = context.EventTypes.Find(eventTypeIds[i]);
                if (venue == null || eventType == null) continue;

                var client = clients[i % clients.Count];

                DateTime start = DateTime.Today.AddDays(-(10 + i * 8)).AddHours(17);
                DateTime end = start.AddHours(4 + (i % 3));

                int quantity = BookingService.CalculateRentalQuantity(venue.RentalUnit, start, end);
                decimal venueCost = venue.RentalRate * quantity;

                var booking = new Booking
                {
                    ClientUserId = client.UserId,
                    VenueId = venue.VenueId,
                    EventTypeId = eventType.EventTypeId,
                    GuestCount = Math.Min(venue.Capacity, 60 + i * 15),
                    EventStartDateTime = start,
                    EventEndDateTime = end,
                    Status = BookingStatuses.Completed,
                    VenueRateAtBooking = venue.RentalRate,
                    RentalUnitAtBooking = venue.RentalUnit,
                    RentalQuantity = quantity,
                    VenueCost = venueCost,
                    CreatedAt = start.AddDays(-14),
                    LastModifiedByUserId = staff.UserId,
                    LastModifiedAt = end
                };

                decimal addOnCost = 0;
                if (addOnIds[i] > 0)
                {
                    var addOn = context.AddOnServices.Find(addOnIds[i]);
                    if (addOn != null)
                    {
                        booking.BookingAddOns.Add(new BookingAddOn
                        {
                            AddOnId = addOn.AddOnId,
                            Quantity = 1,
                            PriceAtBooking = addOn.Price,
                            Subtotal = addOn.Price
                        });
                        addOnCost = addOn.Price;
                    }
                }
                booking.AddOnCost = addOnCost;
                booking.TotalCost = venueCost + addOnCost;

                decimal deposit = Math.Round(booking.TotalCost * BookingService.DepositRate, 0, MidpointRounding.AwayFromZero);
                string method = PaymentMethods.All[i % PaymentMethods.All.Length];

                booking.Payments.Add(new Payment
                {
                    Amount = deposit,
                    PaymentType = PaymentTypes.Deposit,
                    PaymentMethod = method,
                    PaymentStatus = PaymentStatuses.Completed,
                    PaymentDate = booking.CreatedAt.AddDays(1),
                    RecordedByUserId = staff.UserId
                });
                booking.Payments.Add(new Payment
                {
                    Amount = booking.TotalCost - deposit,
                    PaymentType = PaymentTypes.Final,
                    PaymentMethod = method,
                    PaymentStatus = PaymentStatuses.Completed,
                    PaymentDate = start.AddHours(-3),
                    RecordedByUserId = staff.UserId
                });

                context.Bookings.Add(booking);
                context.Feedbacks.Add(new Feedback
                {
                    Booking = booking,
                    Rating = (byte)ratings[i],
                    Comment = comments[i],
                    CreatedAt = end.AddDays(1)
                });
            }

            context.SaveChanges();
        }
    }
}