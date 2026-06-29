using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    public partial class AddCertificateEmailSentAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Certificates"" ADD COLUMN IF NOT EXISTS ""EmailSentAt"" timestamptz;
            ");

            // Treat existing certificates as already notified (avoid duplicate blast on first enable).
            migrationBuilder.Sql(@"
                UPDATE ""Certificates""
                SET ""EmailSentAt"" = ""IssuedAt""
                WHERE ""EmailSentAt"" IS NULL;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Certificates"" DROP COLUMN IF EXISTS ""EmailSentAt"";
            ");
        }
    }
}
