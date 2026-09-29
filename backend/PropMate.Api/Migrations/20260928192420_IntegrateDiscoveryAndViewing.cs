using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PropMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class IntegrateDiscoveryAndViewing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Favourites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyListingId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favourites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favourites_PropertyListings_PropertyListingId",
                        column: x => x.PropertyListingId,
                        principalTable: "PropertyListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favourites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViewingSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyListingId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewingSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViewingSlots_PropertyListings_PropertyListingId",
                        column: x => x.PropertyListingId,
                        principalTable: "PropertyListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ViewingBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ViewingSlotId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BookedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewingBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViewingBookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ViewingBookings_ViewingSlots_ViewingSlotId",
                        column: x => x.ViewingSlotId,
                        principalTable: "ViewingSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favourites_PropertyListingId",
                table: "Favourites",
                column: "PropertyListingId");

            migrationBuilder.CreateIndex(
                name: "IX_Favourites_UserId_PropertyListingId",
                table: "Favourites",
                columns: new[] { "UserId", "PropertyListingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ViewingBookings_UserId",
                table: "ViewingBookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewingBookings_ViewingSlotId",
                table: "ViewingBookings",
                column: "ViewingSlotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ViewingSlots_PropertyListingId",
                table: "ViewingSlots",
                column: "PropertyListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favourites");

            migrationBuilder.DropTable(
                name: "ViewingBookings");

            migrationBuilder.DropTable(
                name: "ViewingSlots");
        }
    }
}
