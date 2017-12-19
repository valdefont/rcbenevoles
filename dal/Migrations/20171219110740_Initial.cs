using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace dal.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Centres",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Adresse = table.Column<string>(nullable: false),
                    Nom = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Centres", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Frais",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Annee = table.Column<int>(nullable: false),
                    TauxKilometrique = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frais", x => x.ID);
                    table.UniqueConstraint("UQ_Frais_Annee", x => x.Annee);
                });

            migrationBuilder.CreateTable(
                name: "Benevoles",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AdresseLigne1 = table.Column<string>(nullable: false),
                    AdresseLigne2 = table.Column<string>(nullable: true),
                    AdresseLigne3 = table.Column<string>(nullable: true),
                    CentreID = table.Column<int>(nullable: false),
                    CodePostal = table.Column<string>(nullable: false),
                    Nom = table.Column<string>(nullable: false),
                    Prenom = table.Column<string>(nullable: false),
                    Telephone = table.Column<string>(nullable: false),
                    Ville = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Benevoles", x => x.ID);
                    table.UniqueConstraint("UQ_Benevole_NomPrenom", x => new { x.Nom, x.Prenom });
                    table.ForeignKey(
                        name: "FK_Benevoles_Centres_CentreID",
                        column: x => x.CentreID,
                        principalTable: "Centres",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CentreID = table.Column<int>(nullable: true),
                    Login = table.Column<string>(nullable: false),
                    Password = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.ID);
                    table.UniqueConstraint("UQ_Utilisateur_Login", x => x.Login);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Centres_CentreID",
                        column: x => x.CentreID,
                        principalTable: "Centres",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pointages",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    BenevoleID = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Distance = table.Column<decimal>(nullable: false),
                    NbDemiJournees = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pointages", x => x.ID);
                    table.UniqueConstraint("UQ_Pointage_Benevole", x => new { x.BenevoleID, x.Date });
                    table.ForeignKey(
                        name: "FK_Pointages_Benevoles_BenevoleID",
                        column: x => x.BenevoleID,
                        principalTable: "Benevoles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Benevoles_CentreID",
                table: "Benevoles",
                column: "CentreID");

            migrationBuilder.CreateIndex(
                name: "IX_Benevoles_Nom_Prenom",
                table: "Benevoles",
                columns: new[] { "Nom", "Prenom" });

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_CentreID",
                table: "Utilisateurs",
                column: "CentreID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Frais");

            migrationBuilder.DropTable(
                name: "Pointages");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Benevoles");

            migrationBuilder.DropTable(
                name: "Centres");
        }
    }
}
