using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Resources",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Resources",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "INR");

            migrationBuilder.AddColumn<string[]>(
                name: "Tags",
                table: "Resources",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'::text[]");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Resources",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Resources",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Resources",
                type: "timestamptz",
                nullable: true);

            // Backfill slugs for existing resources from their titles
            migrationBuilder.Sql(@"
                UPDATE ""Resources""
                SET ""Slug"" = TRIM('-' FROM REGEXP_REPLACE(LOWER(TRIM(""Title"")), '[^a-z0-9]+', '-', 'g'))
                WHERE ""Slug"" = '';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Slug",
                table: "Resources",
                column: "Slug");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Resources_Slug",
                table: "Resources");

            migrationBuilder.DropColumn(name: "Slug", table: "Resources");
            migrationBuilder.DropColumn(name: "Currency", table: "Resources");
            migrationBuilder.DropColumn(name: "Tags", table: "Resources");
            migrationBuilder.DropColumn(name: "DurationMinutes", table: "Resources");
            migrationBuilder.DropColumn(name: "ViewCount", table: "Resources");
            migrationBuilder.DropColumn(name: "PublishedAt", table: "Resources");
        }
    }
}
