using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "PropertyListings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "PropertyListings",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "PropertyListings");
        }
    }
}
