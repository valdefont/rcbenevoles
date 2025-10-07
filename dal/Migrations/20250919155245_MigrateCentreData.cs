using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dal.Migrations
{
    /// <inheritdoc />
    public partial class MigrateCentreData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
        UPDATE ""Centre""
        SET 
            ""CodePostal"" = SUBSTRING(""Rue"" FROM '68[0-9]{3}'),
            ""Commune"" = TRIM(SUBSTRING(""Rue"" FROM '68[0-9]{3}\s+(.*)'))
        WHERE ""Rue"" ~ '68[0-9]{3}';
    ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
