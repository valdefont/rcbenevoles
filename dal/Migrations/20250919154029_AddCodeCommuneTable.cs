using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace dal.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeCommuneTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
              name: "Adresse",
              table: "Centres",
              newName: "Rue");

            migrationBuilder.AddColumn<string>(
                name: "CodePostal",
                table: "Centres",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Commune",
                table: "Centres",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EMail",
                table: "Centres",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telephone",
                table: "Centres",
                type: "text",
                nullable: true);
       

        migrationBuilder.CreateTable(
                name: "CodeCommune",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodecommuneINSEE = table.Column<int>(name: "Code_commune_INSEE", type: "integer", nullable: false),
                    Nomdelacommune = table.Column<string>(name: "Nom_Commune", type: "character varying(255)", maxLength: 255, nullable: true),
                    Codepostal = table.Column<string>(name: "Code_postal", type: "character varying(10)", maxLength: 10, nullable: true),
                    Libelleacheminement = table.Column<string>(name: "Libelle_acheminement", type: "character varying(255)", maxLength: 255, nullable: true),
                    Ligne5 = table.Column<string>(name: "SousCommune", type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeCommune", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeCommune_ID",
                table: "CodeCommune",
                column: "ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeCommune");

            migrationBuilder.DropColumn(
               name: "CodePostal",
               table: "Centres");

            migrationBuilder.DropColumn(
                name: "Commune",
                table: "Centres");

            migrationBuilder.DropColumn(
                name: "EMail",
                table: "Centres");

            migrationBuilder.DropColumn(
                name: "Telephone",
                table: "Centres");

            migrationBuilder.RenameColumn(
                name: "Rue",
                table: "Centres",
                newName: "Adresse");
        }
    }
}
