using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAiApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AiApproved",
                table: "MaintenanceRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiApprovedAt",
                table: "MaintenanceRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiApprovedBy",
                table: "MaintenanceRequests",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiApproved",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "AiApprovedAt",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "AiApprovedBy",
                table: "MaintenanceRequests");
        }
    }
}
