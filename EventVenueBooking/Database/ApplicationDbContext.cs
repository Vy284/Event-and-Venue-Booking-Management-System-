using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using EventVenueBooking.Entities;
using EventVenueBooking.Migrations;

namespace EventVenueBooking.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("name=EventVenueBooking")
        {
            this.Configuration.LazyLoadingEnabled = true;

            System.Data.Entity.Database.SetInitializer(
        new MigrateDatabaseToLatestVersion<ApplicationDbContext, EventVenueBooking.Migrations.Configuration>()
    );
        }

        public DbSet<User> Users { get; set; }
        public DbSet<VenueType> VenueTypes { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<AddOnService> AddOnServices { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<VenueImage> VenueImages { get; set; }
        public DbSet<VenueFacility> VenueFacilities { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingAddOn> BookingAddOns { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Giữ nguyên tên bảng theo class (không tự động đổi thành số nhiều)
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            //Cấu hình mối quan hệ 1 - 0..1 giữa Booking và Feedback
            modelBuilder.Entity<Feedback>()
                .HasRequired(f => f.Booking)
                .WithMany()
                .HasForeignKey(f => f.BookingId)
                .WillCascadeOnDelete(false);

            // Khóa chính phức hợp cho BookingAddOn
            modelBuilder.Entity<BookingAddOn>()
                .HasKey(ba => new { ba.BookingId, ba.AddOnId });

            // Khóa chính phức hợp cho VenueFacility
            modelBuilder.Entity<VenueFacility>()
                .HasKey(vf => new { vf.VenueId, vf.FacilityId });

            // Cấu hình N-N trực tiếp giữa Venue và Facility (Map vào bảng VenueFacilities)
            modelBuilder.Entity<Venue>()
                .HasMany(v => v.Facilities)
                .WithMany(f => f.Venues)
                .Map(cs =>
                {
                    cs.MapLeftKey("VenueId");
                    cs.MapRightKey("FacilityId");
                    cs.ToTable("VenueFacilities");
                });

            // Tắt Cascade Delete cho các quan hệ multiple-path tới User để tránh lỗi SQL Server
            modelBuilder.Entity<Booking>()
                .HasRequired(b => b.ClientUser)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.ClientUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Booking>()
                .HasOptional(b => b.LastModifiedByUser)
                .WithMany(u => u.ModifiedBookings)
                .HasForeignKey(b => b.LastModifiedByUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Booking>()
                .HasOptional(b => b.CancelledByUser)
                .WithMany(u => u.CancelledBookings)
                .HasForeignKey(b => b.CancelledByUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Venue>()
                .HasRequired(v => v.CreatedByUser)
                .WithMany(u => u.CreatedVenues)
                .HasForeignKey(v => v.CreatedByUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.RecordedByUser)
                .WithMany(u => u.RecordedPayments)
                .HasForeignKey(p => p.RecordedByUserId)
                .WillCascadeOnDelete(false);
        }
    }
}