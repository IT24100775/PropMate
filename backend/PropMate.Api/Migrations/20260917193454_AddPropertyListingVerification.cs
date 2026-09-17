using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PropMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyListingVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyListingVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyListingId = table.Column<int>(type: "integer", nullable: false),
                    Recommendation = table.Column<string>(type: "text", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    RiskScore = table.Column<double>(type: "double precision", nullable: false),
                    ReasonsJson = table.Column<string>(type: "text", nullable: false),
                    EvidenceJson = table.Column<string>(type: "text", nullable: false),
                    RequiresHumanReview = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyListingVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyListingVerifications_PropertyListings_PropertyListi~",
                        column: x => x.PropertyListingId,
                        principalTable: "PropertyListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyListingVerifications_PropertyListingId",
                table: "PropertyListingVerifications",
                column: "PropertyListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyListingVerifications");
        }
    }
}
