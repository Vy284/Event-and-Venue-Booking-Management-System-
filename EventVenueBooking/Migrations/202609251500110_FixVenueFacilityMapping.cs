namespace EventVenueBooking.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class FixVenueFacilityMapping : DbMigration
    {
        public override void Up()
        {
            // 1. Remove old foreign keys from the two junction tables
            DropForeignKey("dbo.VenueFacilities", "VenueId", "dbo.Venue");
            DropForeignKey("dbo.VenueFacilities", "FacilityId", "dbo.Facility");

            DropForeignKey("dbo.VenueFacility", "VenueId", "dbo.Venue");
            DropForeignKey("dbo.VenueFacility", "FacilityId", "dbo.Facility");

            // 2. Remove the old N:N junction table.
            //    This table is empty in the current database.
            DropTable("dbo.VenueFacilities");

            // 3. Rename the explicit junction table
            //    VenueFacility -> VenueFacilities
            RenameTable(
                name: "dbo.VenueFacility",
                newName: "VenueFacilities"
            );

            // 4. Rename entity tables to their plural names
            RenameTable(name: "dbo.AddOnService", newName: "AddOnServices");
            RenameTable(name: "dbo.BookingAddOn", newName: "BookingAddOns");
            RenameTable(name: "dbo.Booking", newName: "Bookings");
            RenameTable(name: "dbo.User", newName: "Users");
            RenameTable(name: "dbo.Venue", newName: "Venues");
            RenameTable(name: "dbo.Facility", newName: "Facilities");
            RenameTable(name: "dbo.VenueImage", newName: "VenueImages");
            RenameTable(name: "dbo.VenueType", newName: "VenueTypes");
            RenameTable(name: "dbo.Payment", newName: "Payments");
            RenameTable(name: "dbo.EventType", newName: "EventTypes");
            RenameTable(name: "dbo.Feedback", newName: "Feedbacks");

            // 5. Re-create VenueFacility foreign keys
            //    using the new plural table names and without cascade delete.
            AddForeignKey(
                "dbo.VenueFacilities",
                "VenueId",
                "dbo.Venues",
                "VenueId",
                cascadeDelete: false
            );

            AddForeignKey(
                "dbo.VenueFacilities",
                "FacilityId",
                "dbo.Facilities",
                "FacilityId",
                cascadeDelete: false
            );
        }

        public override void Down()
        {
            // Reverse the changes made in Up()

            DropForeignKey(
                "dbo.VenueFacilities",
                "VenueId",
                "dbo.Venues"
            );

            DropForeignKey(
                "dbo.VenueFacilities",
                "FacilityId",
                "dbo.Facilities"
            );

            // Rename plural tables back to their original names
            RenameTable(name: "dbo.Feedbacks", newName: "Feedback");
            RenameTable(name: "dbo.EventTypes", newName: "EventType");
            RenameTable(name: "dbo.Payments", newName: "Payment");
            RenameTable(name: "dbo.VenueTypes", newName: "VenueType");
            RenameTable(name: "dbo.VenueImages", newName: "VenueImage");
            RenameTable(name: "dbo.Facilities", newName: "Facility");
            RenameTable(name: "dbo.Venues", newName: "Venue");
            RenameTable(name: "dbo.Users", newName: "User");
            RenameTable(name: "dbo.Bookings", newName: "Booking");
            RenameTable(name: "dbo.BookingAddOns", newName: "BookingAddOn");
            RenameTable(name: "dbo.AddOnServices", newName: "AddOnService");

            // Rename junction table back
            RenameTable(
                name: "dbo.VenueFacilities",
                newName: "VenueFacility"
            );

            // Restore the old foreign keys
            AddForeignKey(
                "dbo.VenueFacility",
                "VenueId",
                "dbo.Venue",
                "VenueId",
                cascadeDelete: true
            );

            AddForeignKey(
                "dbo.VenueFacility",
                "FacilityId",
                "dbo.Facility",
                "FacilityId",
                cascadeDelete: true
            );

            // Re-create the old N:N table
            CreateTable(
                "dbo.VenueFacilities",
                c => new
                {
                    VenueId = c.Int(nullable: false),
                    FacilityId = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.VenueId, t.FacilityId });

            AddForeignKey(
                "dbo.VenueFacilities",
                "VenueId",
                "dbo.Venue",
                "VenueId",
                cascadeDelete: true
            );

            AddForeignKey(
                "dbo.VenueFacilities",
                "FacilityId",
                "dbo.Facility",
                "FacilityId",
                cascadeDelete: true
            );
        }
    }
}