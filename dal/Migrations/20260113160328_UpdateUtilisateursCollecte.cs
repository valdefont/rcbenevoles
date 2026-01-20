using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUtilisateursCollecte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "app_collecte",
                table: "Utilisateurs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
               name: "app_collecte",
               table: "Utilisateurs");

        }
    }
}
