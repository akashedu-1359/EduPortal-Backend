using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "CmsBanners",
                type: "text",
                nullable: false,
                defaultValue: "Hero");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "CmsBanners");
        }
    }
}
