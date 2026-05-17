using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    public partial class EnsureResourceColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent — safe to run even if previous migration partially applied
            migrationBuilder.Sql(@"
                ALTER TABLE ""Resources"" ADD COLUMN IF NOT EXISTS ""Slug"" character varying(300) NOT NULL DEFAULT '';
                ALTER TABLE ""Resources"" ADD COLUMN IF NOT EXISTS ""Currency"" character varying(10) NOT NULL DEFAULT 'INR';
                ALTER TABLE ""Resources"" ADD COLUMN IF NOT EXISTS ""Tags"" text[] NOT NULL DEFAULT '{}';
                ALTER TABLE ""Resources"" ADD COLUMN IF NOT EXISTS ""DurationMinutes"" integer;
                ALTER TABLE ""Resources"" ADD COLUMN IF NOT EXISTS ""ViewCount"" integer NOT NULL DEFAULT 0;
                ALTER TABLE ""Resources"" ADD COLUMN IF NOT EXISTS ""PublishedAt"" timestamptz;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Resources""
                SET ""Slug"" = TRIM('-' FROM REGEXP_REPLACE(LOWER(TRIM(""Title"")), '[^a-z0-9]+', '-', 'g'))
                WHERE ""Slug"" = '';
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_Resources_Slug"" ON ""Resources"" (""Slug"");
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Resources_Slug"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Resources"" DROP COLUMN IF EXISTS ""Slug"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Resources"" DROP COLUMN IF EXISTS ""Currency"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Resources"" DROP COLUMN IF EXISTS ""Tags"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Resources"" DROP COLUMN IF EXISTS ""DurationMinutes"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Resources"" DROP COLUMN IF EXISTS ""ViewCount"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Resources"" DROP COLUMN IF EXISTS ""PublishedAt"";");
        }
    }
}
