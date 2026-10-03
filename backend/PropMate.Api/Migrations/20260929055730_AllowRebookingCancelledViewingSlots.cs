using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class AllowRebookingCancelledViewingSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ViewingBookings_ViewingSlotId",
                table: "ViewingBookings");

            migrationBuilder.CreateIndex(
                name: "IX_ViewingBookings_ViewingSlotId",
                table: "ViewingBookings",
                column: "ViewingSlotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ViewingBookings_ViewingSlotId",
                table: "ViewingBookings");

            migrationBuilder.CreateIndex(
                name: "IX_ViewingBookings_ViewingSlotId",
                table: "ViewingBookings",
                column: "ViewingSlotId",
                unique: true);
        }
    }
}
