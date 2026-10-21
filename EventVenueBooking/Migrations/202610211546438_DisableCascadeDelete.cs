namespace EventVenueBooking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DisableCascadeDelete : DbMigration
    {
        public override void Up()
        {
            // Xóa FK cũ bằng đúng tên đang có trong DB
            DropForeignKey("dbo.Venues", "FK_dbo.Venue_dbo.VenueType_VenueTypeId");
            DropForeignKey("dbo.Bookings", "FK_dbo.Booking_dbo.Venue_VenueId");
            DropForeignKey("dbo.Bookings", "FK_dbo.Booking_dbo.EventType_EventTypeId");
            DropForeignKey("dbo.Payments", "FK_dbo.Payment_dbo.Booking_BookingId");
            DropForeignKey("dbo.BookingAddOns", "FK_dbo.BookingAddOn_dbo.AddOnService_AddOnId");

            // Tạo lại không cascade
            AddForeignKey("dbo.Venues", "VenueTypeId", "dbo.VenueTypes", "VenueTypeId", cascadeDelete: false);
            AddForeignKey("dbo.Bookings", "VenueId", "dbo.Venues", "VenueId", cascadeDelete: false);
            AddForeignKey("dbo.Bookings", "EventTypeId", "dbo.EventTypes", "EventTypeId", cascadeDelete: false);
            AddForeignKey("dbo.Payments", "BookingId", "dbo.Bookings", "BookingId", cascadeDelete: false);
            AddForeignKey("dbo.BookingAddOns", "AddOnId", "dbo.AddOnServices", "AddOnId", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BookingAddOns", "AddOnId", "dbo.AddOnServices");
            DropForeignKey("dbo.Payments", "BookingId", "dbo.Bookings");
            DropForeignKey("dbo.Bookings", "EventTypeId", "dbo.EventTypes");
            DropForeignKey("dbo.Bookings", "VenueId", "dbo.Venues");
            DropForeignKey("dbo.Venues", "VenueTypeId", "dbo.VenueTypes");

            AddForeignKey("dbo.BookingAddOns", "AddOnId", "dbo.AddOnServices", "AddOnId", cascadeDelete: true);
            AddForeignKey("dbo.Payments", "BookingId", "dbo.Bookings", "BookingId", cascadeDelete: true);
            AddForeignKey("dbo.Bookings", "EventTypeId", "dbo.EventTypes", "EventTypeId", cascadeDelete: true);
            AddForeignKey("dbo.Bookings", "VenueId", "dbo.Venues", "VenueId", cascadeDelete: true);
            AddForeignKey("dbo.Venues", "VenueTypeId", "dbo.VenueTypes", "VenueTypeId", cascadeDelete: true);
        }
    }
}
