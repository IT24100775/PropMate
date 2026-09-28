using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PropMate.Api.Data;

#nullable disable

namespace PropMate.Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260928120000_RemoveAuthenticationAndSeedDemoWorkspaces")]
public partial class RemoveAuthenticationAndSeedDemoWorkspaces : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PasswordHash", table: "Users");
        migrationBuilder.DropColumn(name: "IsActive", table: "Users");

        migrationBuilder.Sql("""
            INSERT INTO "Users" ("Id", "FirstName", "LastName", "Email", "Role", "IsVerified", "CreatedAt", "UpdatedAt")
            VALUES
                (1, 'Demo', 'Tenant', 'tenant@propmate.demo', 0, TRUE, '2026-09-28T00:00:00Z', '2026-09-28T00:00:00Z'),
                (2, 'Demo', 'Owner', 'owner@propmate.demo', 1, TRUE, '2026-09-28T00:00:00Z', '2026-09-28T00:00:00Z'),
                (3, 'Demo', 'Admin', 'admin@propmate.demo', 2, TRUE, '2026-09-28T00:00:00Z', '2026-09-28T00:00:00Z'),
                (4, 'Demo', 'Manager', 'manager@propmate.demo', 3, TRUE, '2026-09-28T00:00:00Z', '2026-09-28T00:00:00Z')
            ON CONFLICT ("Id") DO NOTHING;
            SELECT setval(pg_get_serial_sequence('"Users"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "Users"), 1), TRUE);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM \"Users\" WHERE \"Id\" IN (1, 2, 3, 4);");

        migrationBuilder.AddColumn<string>(
            name: "PasswordHash",
            table: "Users",
            type: "text",
            nullable: false,
            defaultValue: "");
        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "Users",
            type: "boolean",
            nullable: false,
            defaultValue: true);
    }
}
