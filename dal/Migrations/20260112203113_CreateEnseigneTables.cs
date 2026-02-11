using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace dal.Migrations
{
    /// <inheritdoc />
    public partial class CreateEnseigneTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Enseigne",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enseigne", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EnseigneDetail",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnseigneID = table.Column<int>(type: "integer", nullable: false),
                    CodeCommuneID = table.Column<int>(type: "integer", nullable: false),
                    CentreID = table.Column<int>(type: "integer", nullable: false),
                    Adresse = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),                    
                    EstActif = table.Column<bool>(type: "boolean", nullable: false ,defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnseigneDetail", x => x.ID);
                   
                    table.ForeignKey(
                        name: "FK_EnseigneDetail_Centres_CentreID",
                        column: x => x.CentreID,
                        principalTable: "Centres",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnseigneDetail_CodeCommune_CodeCommuneID",
                        column: x => x.CodeCommuneID,
                        principalTable: "CodeCommune",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnseigneDetail_Enseigne_EnseigneID",
                        column: x => x.EnseigneID,
                        principalTable: "Enseigne",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Collecte",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnseigneDetailID = table.Column<int>(type: "integer", nullable: false),
                    Poids = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    UtilisateurID = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collecte", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Collecte_EnseigneDetail_EnseigneDetailID",
                        column: x => x.EnseigneDetailID,
                        principalTable: "EnseigneDetail",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Collecte_Utilisateurs_UtilisateurID",
                        column: x => x.UtilisateurID,
                        principalTable: "Utilisateurs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collecte_EnseigneDetailID_DateCreation",
                table: "Collecte",
                columns: new[] { "EnseigneDetailID", "DateCreation" });

            migrationBuilder.CreateIndex(
               name: "IX_Collecte_DateCreation",
               table: "Collecte",
               column: "DateCreation");

            migrationBuilder.CreateIndex(
                name: "IX_Collecte_UtilisateurID",
                table: "Collecte",
                column: "UtilisateurID");

            migrationBuilder.CreateIndex(
                name: "IX_Enseigne_Name",
                table: "Enseigne",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_EnseigneDetail_Adresse",
                table: "EnseigneDetail",
                column: "Adresse");          

            migrationBuilder.CreateIndex(
                name: "IX_EnseigneDetail_CentreID",
                table: "EnseigneDetail",
                column: "CentreID");

            migrationBuilder.CreateIndex(
                name: "IX_EnseigneDetail_CodeCommuneID",
                table: "EnseigneDetail",
                column: "CodeCommuneID");

            migrationBuilder.CreateIndex(
                name: "IX_EnseigneDetail_EnseigneID",
                table: "EnseigneDetail",
                column: "EnseigneID");

            migrationBuilder.CreateIndex(
                name: "IX_EnseigneDetail_EstActif",
                table: "EnseigneDetail",
                column: "EstActif");


           

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Collecte");

            migrationBuilder.DropTable(
                name: "EnseigneDetail");

            migrationBuilder.DropTable(
                name: "Enseigne");
        }
    }
}
