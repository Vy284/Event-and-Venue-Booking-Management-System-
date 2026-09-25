namespace EventVenueBooking.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AddOnService",
                c => new
                    {
                        AddOnId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        Description = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Category = c.String(maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.AddOnId)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "dbo.BookingAddOn",
                c => new
                    {
                        BookingId = c.Int(nullable: false),
                        AddOnId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        PriceAtBooking = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Subtotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.BookingId, t.AddOnId })
                .ForeignKey("dbo.AddOnService", t => t.AddOnId, cascadeDelete: true)
                .ForeignKey("dbo.Booking", t => t.BookingId, cascadeDelete: true)
                .Index(t => t.BookingId)
                .Index(t => t.AddOnId);
            
            CreateTable(
                "dbo.Booking",
                c => new
                    {
                        BookingId = c.Int(nullable: false, identity: true),
                        ClientUserId = c.Int(nullable: false),
                        VenueId = c.Int(nullable: false),
                        EventTypeId = c.Int(nullable: false),
                        GuestCount = c.Int(nullable: false),
                        EventStartDateTime = c.DateTime(nullable: false),
                        EventEndDateTime = c.DateTime(nullable: false),
                        Status = c.Byte(nullable: false),
                        VenueRateAtBooking = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RentalUnitAtBooking = c.Byte(nullable: false),
                        RentalQuantity = c.Int(nullable: false),
                        VenueCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AddOnCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedAt = c.DateTime(nullable: false),
                        LastModifiedByUserId = c.Int(),
                        LastModifiedAt = c.DateTime(),
                        CancelledByUserId = c.Int(),
                        CancelledAt = c.DateTime(),
                        CancelReason = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.BookingId)
                .ForeignKey("dbo.User", t => t.CancelledByUserId)
                .ForeignKey("dbo.User", t => t.ClientUserId)
                .ForeignKey("dbo.EventType", t => t.EventTypeId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastModifiedByUserId)
                .ForeignKey("dbo.Venue", t => t.VenueId, cascadeDelete: true)
                .Index(t => t.ClientUserId)
                .Index(t => t.VenueId)
                .Index(t => t.EventTypeId)
                .Index(t => t.LastModifiedByUserId)
                .Index(t => t.CancelledByUserId);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 255),
                        PasswordHash = c.String(nullable: false, maxLength: 255),
                        Phone = c.String(maxLength: 20),
                        Role = c.Byte(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .Index(t => t.Email, unique: true);
            
            CreateTable(
                "dbo.Venue",
                c => new
                    {
                        VenueId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        VenueTypeId = c.Int(nullable: false),
                        Capacity = c.Int(nullable: false),
                        Description = c.String(),
                        RentalRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RentalUnit = c.Byte(nullable: false),
                        Location = c.String(nullable: false, maxLength: 255),
                        Status = c.Byte(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.VenueId)
                .ForeignKey("dbo.VenueType", t => t.VenueTypeId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.CreatedByUserId)
                .Index(t => t.VenueTypeId)
                .Index(t => t.CreatedByUserId);
            
            CreateTable(
                "dbo.Facility",
                c => new
                    {
                        FacilityId = c.Int(nullable: false, identity: true),
                        FacilityName = c.String(nullable: false, maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.FacilityId)
                .Index(t => t.FacilityName, unique: true);
            
            CreateTable(
                "dbo.VenueImage",
                c => new
                    {
                        ImageId = c.Int(nullable: false, identity: true),
                        VenueId = c.Int(nullable: false),
                        ImageUrl = c.String(nullable: false, maxLength: 500),
                        IsPrimary = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ImageId)
                .ForeignKey("dbo.Venue", t => t.VenueId, cascadeDelete: true)
                .Index(t => t.VenueId);
            
            CreateTable(
                "dbo.VenueType",
                c => new
                    {
                        VenueTypeId = c.Int(nullable: false, identity: true),
                        TypeName = c.String(nullable: false, maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.VenueTypeId)
                .Index(t => t.TypeName, unique: true);
            
            CreateTable(
                "dbo.Payment",
                c => new
                    {
                        PaymentId = c.Int(nullable: false, identity: true),
                        BookingId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentType = c.Byte(nullable: false),
                        PaymentMethod = c.String(maxLength: 50),
                        PaymentStatus = c.Byte(nullable: false),
                        PaymentDate = c.DateTime(),
                        RecordedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PaymentId)
                .ForeignKey("dbo.Booking", t => t.BookingId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.RecordedByUserId)
                .Index(t => t.BookingId)
                .Index(t => t.RecordedByUserId);
            
            CreateTable(
                "dbo.EventType",
                c => new
                    {
                        EventTypeId = c.Int(nullable: false, identity: true),
                        TypeName = c.String(nullable: false, maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.EventTypeId)
                .Index(t => t.TypeName, unique: true);
            
            CreateTable(
                "dbo.Feedback",
                c => new
                    {
                        FeedbackId = c.Int(nullable: false, identity: true),
                        BookingId = c.Int(nullable: false),
                        Rating = c.Byte(nullable: false),
                        Comment = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.FeedbackId)
                .ForeignKey("dbo.Booking", t => t.BookingId)
                .Index(t => t.BookingId, unique: true);
            
            CreateTable(
                "dbo.VenueFacility",
                c => new
                    {
                        VenueId = c.Int(nullable: false),
                        FacilityId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.VenueId, t.FacilityId })
                .ForeignKey("dbo.Facility", t => t.FacilityId, cascadeDelete: true)
                .ForeignKey("dbo.Venue", t => t.VenueId, cascadeDelete: true)
                .Index(t => t.VenueId)
                .Index(t => t.FacilityId);
            
            CreateTable(
                "dbo.VenueFacilities",
                c => new
                    {
                        VenueId = c.Int(nullable: false),
                        FacilityId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.VenueId, t.FacilityId })
                .ForeignKey("dbo.Venue", t => t.VenueId, cascadeDelete: true)
                .ForeignKey("dbo.Facility", t => t.FacilityId, cascadeDelete: true)
                .Index(t => t.VenueId)
                .Index(t => t.FacilityId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VenueFacility", "VenueId", "dbo.Venue");
            DropForeignKey("dbo.VenueFacility", "FacilityId", "dbo.Facility");
            DropForeignKey("dbo.Feedback", "BookingId", "dbo.Booking");
            DropForeignKey("dbo.BookingAddOn", "BookingId", "dbo.Booking");
            DropForeignKey("dbo.Booking", "VenueId", "dbo.Venue");
            DropForeignKey("dbo.Booking", "LastModifiedByUserId", "dbo.User");
            DropForeignKey("dbo.Booking", "EventTypeId", "dbo.EventType");
            DropForeignKey("dbo.Booking", "ClientUserId", "dbo.User");
            DropForeignKey("dbo.Booking", "CancelledByUserId", "dbo.User");
            DropForeignKey("dbo.Payment", "RecordedByUserId", "dbo.User");
            DropForeignKey("dbo.Payment", "BookingId", "dbo.Booking");
            DropForeignKey("dbo.Venue", "CreatedByUserId", "dbo.User");
            DropForeignKey("dbo.Venue", "VenueTypeId", "dbo.VenueType");
            DropForeignKey("dbo.VenueImage", "VenueId", "dbo.Venue");
            DropForeignKey("dbo.VenueFacilities", "FacilityId", "dbo.Facility");
            DropForeignKey("dbo.VenueFacilities", "VenueId", "dbo.Venue");
            DropForeignKey("dbo.BookingAddOn", "AddOnId", "dbo.AddOnService");
            DropIndex("dbo.VenueFacilities", new[] { "FacilityId" });
            DropIndex("dbo.VenueFacilities", new[] { "VenueId" });
            DropIndex("dbo.VenueFacility", new[] { "FacilityId" });
            DropIndex("dbo.VenueFacility", new[] { "VenueId" });
            DropIndex("dbo.Feedback", new[] { "BookingId" });
            DropIndex("dbo.EventType", new[] { "TypeName" });
            DropIndex("dbo.Payment", new[] { "RecordedByUserId" });
            DropIndex("dbo.Payment", new[] { "BookingId" });
            DropIndex("dbo.VenueType", new[] { "TypeName" });
            DropIndex("dbo.VenueImage", new[] { "VenueId" });
            DropIndex("dbo.Facility", new[] { "FacilityName" });
            DropIndex("dbo.Venue", new[] { "CreatedByUserId" });
            DropIndex("dbo.Venue", new[] { "VenueTypeId" });
            DropIndex("dbo.User", new[] { "Email" });
            DropIndex("dbo.Booking", new[] { "CancelledByUserId" });
            DropIndex("dbo.Booking", new[] { "LastModifiedByUserId" });
            DropIndex("dbo.Booking", new[] { "EventTypeId" });
            DropIndex("dbo.Booking", new[] { "VenueId" });
            DropIndex("dbo.Booking", new[] { "ClientUserId" });
            DropIndex("dbo.BookingAddOn", new[] { "AddOnId" });
            DropIndex("dbo.BookingAddOn", new[] { "BookingId" });
            DropIndex("dbo.AddOnService", new[] { "Name" });
            DropTable("dbo.VenueFacilities");
            DropTable("dbo.VenueFacility");
            DropTable("dbo.Feedback");
            DropTable("dbo.EventType");
            DropTable("dbo.Payment");
            DropTable("dbo.VenueType");
            DropTable("dbo.VenueImage");
            DropTable("dbo.Facility");
            DropTable("dbo.Venue");
            DropTable("dbo.User");
            DropTable("dbo.Booking");
            DropTable("dbo.BookingAddOn");
            DropTable("dbo.AddOnService");
        }
    }
}
