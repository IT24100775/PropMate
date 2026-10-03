using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceAiFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiRecommendedSchedule",
                table: "MaintenanceRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiTechnicianSpecialization",
                table: "MaintenanceRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiRecommendedSchedule",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "AiTechnicianSpecialization",
                table: "MaintenanceRequests");
        }
    }
}
