using BusRejserLibrary.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusRejserLibrary.Migrations
{
    [DbContext(typeof(BusPlanenDbContext))]
    [Migration("20260506183000_SeedProgressionBadges")]
    public partial class SeedProgressionBadges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO badges
                (BadgeName, Description, Country, Region, Municipality, Slug, IconUrl, RuleType, RuleValue, RequiredValue, RuleWindowValue, IsActive, Tier)
                VALUES
                ('Foerste Tur', 'Gennemfoer din foerste betalte rejse med BusPlanen.', NULL, NULL, NULL, 'foerste-tur', '', 'CompletedTrips', NULL, 1, NULL, 1, 1),
                ('Paa Farten', 'Gennemfoer 3 rejser og vis at du er kommet godt i gang.', NULL, NULL, NULL, 'paa-farten', '', 'CompletedTrips', NULL, 3, NULL, 1, 1),
                ('Loyal Rejsende', 'Gennemfoer 10 rejser med BusPlanen.', NULL, NULL, NULL, 'loyal-rejsende', '', 'CompletedTrips', NULL, 10, NULL, 1, 3),
                ('Tidlig Planlaegger', 'Book en rejse mindst 14 dage foer afgang.', NULL, NULL, NULL, 'tidlig-planlaegger', '', 'EarlyBooking', NULL, 14, NULL, 1, 2),
                ('Last Minute Legend', 'Book en rejse mindre end 48 timer foer afgang.', NULL, NULL, NULL, 'last-minute-legend', '', 'LastMinute', NULL, 48, NULL, 1, 2),
                ('Destination Hunter', 'Gennemfoer rejser til 3 forskellige destinationer.', NULL, NULL, NULL, 'destination-hunter', '', 'UniqueDestinations', NULL, 3, NULL, 1, 2),
                ('Back To Back', 'Gennemfoer to rejser med hoejst 48 timer imellem.', NULL, NULL, NULL, 'back-to-back', '', 'BackToBack', NULL, 48, NULL, 1, 3),
                ('Double Trouble', 'Gennemfoer 2 rejser inden for 30 dage.', NULL, NULL, NULL, 'double-trouble', '', 'DoubleTrouble', NULL, 2, 30, 1, 3),
                ('Night Rider', 'Gennemfoer en rejse der starter mellem midnat og klokken fem.', NULL, NULL, NULL, 'night-rider', '', 'NightRider', NULL, 1, NULL, 1, 4);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM badges
                WHERE Slug IN ('foerste-tur', 'paa-farten', 'loyal-rejsende', 'tidlig-planlaegger', 'last-minute-legend', 'destination-hunter', 'back-to-back', 'double-trouble', 'night-rider');
            ");
        }
    }
}
