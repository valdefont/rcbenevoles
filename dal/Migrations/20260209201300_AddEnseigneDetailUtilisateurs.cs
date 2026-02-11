using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dal.Migrations
{
    /// <inheritdoc />
    public partial class AddEnseigneDetailUtilisateurs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnseigneDetailUtilisateurs",
                columns: table => new
                {
                    EnseigneDetailID = table.Column<int>(type: "integer", nullable: false),
                    UtilisateurID = table.Column<int>(type: "integer", nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    CreeLe = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnseigneDetailUtilisateurs", x => new { x.EnseigneDetailID, x.UtilisateurID });
                    table.ForeignKey(
                        name: "FK_EnseigneDetailUtilisateurs_EnseigneDetail_EnseigneDetailID",
                        column: x => x.EnseigneDetailID,
                        principalTable: "EnseigneDetail",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnseigneDetailUtilisateurs_Utilisateurs_UtilisateurID",
                        column: x => x.UtilisateurID,
                        principalTable: "Utilisateurs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnseigneDetailUtilisateurs_EnseigneDetailID",
                table: "EnseigneDetailUtilisateurs",
                column: "EnseigneDetailID");

            migrationBuilder.CreateIndex(
                name: "IX_EnseigneDetailUtilisateurs_EstActif",
                table: "EnseigneDetailUtilisateurs",
                column: "EstActif");

            migrationBuilder.CreateIndex(
                name: "IX_EnseigneDetailUtilisateurs_UtilisateurID",
                table: "EnseigneDetailUtilisateurs",
                column: "UtilisateurID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnseigneDetailUtilisateurs");
        }
    }
}
