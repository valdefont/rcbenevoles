using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace dal.Migrations
{
    /// <inheritdoc />
    public partial class dbChangeForBonLivraison : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.AddColumn<bool>(
                name: "app_bon_livraison",
                table: "Utilisateurs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "app_pointage_benevoles",
                table: "Utilisateurs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BonLivraison",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumBulletin = table.Column<string>(type: "text", nullable: true),
                    CentreID = table.Column<int>(type: "integer", nullable: false),
                    DateLivraison = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Poids = table.Column<int>(type: "integer", nullable: false),
                    UtilisateurID = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonLivraison", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BonLivraison_Centres_CentreID",
                        column: x => x.CentreID,
                        principalTable: "Centres",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BonLivraison_Utilisateurs_UtilisateurID",
                        column: x => x.UtilisateurID,
                        principalTable: "Utilisateurs",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BonLivraison_CentreID",
                table: "BonLivraison",
                column: "CentreID");

            migrationBuilder.CreateIndex(
                name: "IX_BonLivraison_UtilisateurID",
                table: "BonLivraison",
                column: "UtilisateurID");*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BonLivraison");

            migrationBuilder.DropColumn(
                name: "app_bon_livraison",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "app_pointage_benevoles",
                table: "Utilisateurs");
        }
    }
}
