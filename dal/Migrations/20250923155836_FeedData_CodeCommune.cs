using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dal.Migrations
{
    /// <inheritdoc />
    public partial class FeedDataCodeCommune : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

           

            migrationBuilder.Sql(@"ALTER TABLE ""CodeCommune"" ALTER COLUMN ""Code_commune_INSEE"" TYPE VARCHAR(10);");
            /*
            */

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
