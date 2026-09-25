using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PropMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class Component3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyListingId = table.Column<int>(type: "integer", nullable: false),
                    BuyerId = table.Column<int>(type: "integer", nullable: false),
                    OfferAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Conditions = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    NegotiationStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOffers_PropertyListings_PropertyListingId",
                        column: x => x.PropertyListingId,
                        principalTable: "PropertyListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOffers_Users_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RentalApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyListingId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Employment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MonthlyIncome = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Occupants = table.Column<int>(type: "integer", nullable: false),
                    PreferredMoveInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationMonths = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    NegotiationStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalApplications_PropertyListings_PropertyListingId",
                        column: x => x.PropertyListingId,
                        principalTable: "PropertyListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalApplications_Users_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseAgreements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOfferId = table.Column<int>(type: "integer", nullable: false),
                    PropertyListingId = table.Column<int>(type: "integer", nullable: false),
                    SellerId = table.Column<int>(type: "integer", nullable: false),
                    BuyerId = table.Column<int>(type: "integer", nullable: false),
                    FinalPurchasePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Conditions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    BuyerObligation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SellerObligation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PenaltyTerms = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    BuyerConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    BuyerConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SellerConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    SellerConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseAgreements_PurchaseOffers_PurchaseOfferId",
                        column: x => x.PurchaseOfferId,
                        principalTable: "PurchaseOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseNegotiationMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOfferId = table.Column<int>(type: "integer", nullable: false),
                    SenderUserId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseNegotiationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseNegotiationMessages_PurchaseOffers_PurchaseOfferId",
                        column: x => x.PurchaseOfferId,
                        principalTable: "PurchaseOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseNegotiationMessages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseNegotiationOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOfferId = table.Column<int>(type: "integer", nullable: false),
                    ProposedByUserId = table.Column<int>(type: "integer", nullable: false),
                    OfferAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Conditions = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseNegotiationOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseNegotiationOffers_PurchaseOffers_PurchaseOfferId",
                        column: x => x.PurchaseOfferId,
                        principalTable: "PurchaseOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseNegotiationOffers_Users_ProposedByUserId",
                        column: x => x.ProposedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RentalAgreements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RentalApplicationId = table.Column<int>(type: "integer", nullable: false),
                    PropertyListingId = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    FinalMonthlyRent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MoveInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationMonths = table.Column<int>(type: "integer", nullable: false),
                    Terms = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    TenantObligation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OwnerObligation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PenaltyTerms = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    BuyerConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    BuyerConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SellerConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    SellerConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalAgreements_RentalApplications_RentalApplicationId",
                        column: x => x.RentalApplicationId,
                        principalTable: "RentalApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RentalNegotiationMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RentalApplicationId = table.Column<int>(type: "integer", nullable: false),
                    SenderUserId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalNegotiationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalNegotiationMessages_RentalApplications_RentalApplicat~",
                        column: x => x.RentalApplicationId,
                        principalTable: "RentalApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RentalNegotiationMessages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RentalNegotiationOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RentalApplicationId = table.Column<int>(type: "integer", nullable: false),
                    ProposedByUserId = table.Column<int>(type: "integer", nullable: false),
                    MonthlyRent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MoveInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationMonths = table.Column<int>(type: "integer", nullable: false),
                    Conditions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalNegotiationOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalNegotiationOffers_RentalApplications_RentalApplicatio~",
                        column: x => x.RentalApplicationId,
                        principalTable: "RentalApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RentalNegotiationOffers_Users_ProposedByUserId",
                        column: x => x.ProposedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseAgreements_PurchaseOfferId",
                table: "PurchaseAgreements",
                column: "PurchaseOfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseNegotiationMessages_PurchaseOfferId",
                table: "PurchaseNegotiationMessages",
                column: "PurchaseOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseNegotiationMessages_SenderUserId",
                table: "PurchaseNegotiationMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseNegotiationOffers_ProposedByUserId",
                table: "PurchaseNegotiationOffers",
                column: "ProposedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseNegotiationOffers_PurchaseOfferId",
                table: "PurchaseNegotiationOffers",
                column: "PurchaseOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOffers_BuyerId",
                table: "PurchaseOffers",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOffers_PropertyListingId",
                table: "PurchaseOffers",
                column: "PropertyListingId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOffers_Status",
                table: "PurchaseOffers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RentalAgreements_RentalApplicationId",
                table: "RentalAgreements",
                column: "RentalApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RentalApplications_PropertyListingId",
                table: "RentalApplications",
                column: "PropertyListingId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalApplications_Status",
                table: "RentalApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RentalApplications_TenantId",
                table: "RentalApplications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalNegotiationMessages_RentalApplicationId",
                table: "RentalNegotiationMessages",
                column: "RentalApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalNegotiationMessages_SenderUserId",
                table: "RentalNegotiationMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalNegotiationOffers_ProposedByUserId",
                table: "RentalNegotiationOffers",
                column: "ProposedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalNegotiationOffers_RentalApplicationId",
                table: "RentalNegotiationOffers",
                column: "RentalApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseAgreements");

            migrationBuilder.DropTable(
                name: "PurchaseNegotiationMessages");

            migrationBuilder.DropTable(
                name: "PurchaseNegotiationOffers");

            migrationBuilder.DropTable(
                name: "RentalAgreements");

            migrationBuilder.DropTable(
                name: "RentalNegotiationMessages");

            migrationBuilder.DropTable(
                name: "RentalNegotiationOffers");

            migrationBuilder.DropTable(
                name: "PurchaseOffers");

            migrationBuilder.DropTable(
                name: "RentalApplications");
        }
    }
}
