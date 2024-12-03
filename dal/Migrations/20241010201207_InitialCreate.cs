using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace dal.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaremeFiscalDefault",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NbChevaux = table.Column<int>(type: "integer", nullable: false),
                    LimiteKm = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaremeFiscalDefault", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "BaremeFiscalLignes",
                columns: table => new
                {
                    Annee = table.Column<int>(type: "integer", nullable: false),
                    NbChevaux = table.Column<int>(type: "integer", nullable: false),
                    LimiteKm = table.Column<int>(type: "integer", nullable: false),
                    Coef = table.Column<decimal>(type: "numeric", nullable: false),
                    Ajout = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaremeFiscalLignes", x => new { x.Annee, x.NbChevaux, x.LimiteKm });
                });

            migrationBuilder.CreateTable(
                name: "Benevoles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "text", nullable: false),
                    Prenom = table.Column<string>(type: "text", nullable: false),
                    Telephone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Benevoles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Frais",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Annee = table.Column<int>(type: "integer", nullable: false),
                    TauxKilometrique = table.Column<decimal>(type: "numeric", nullable: false),
                    PourcentageVehiculeElectrique = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frais", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Sieges",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "text", nullable: false),
                    Adresse = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sieges", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Vehicule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NbChevaux = table.Column<int>(type: "integer", nullable: false),
                    BenevoleID = table.Column<int>(type: "integer", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    DateChangement = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsElectric = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicule", x => x.id);
                    table.ForeignKey(
                        name: "FK_Vehicule_Benevoles_BenevoleID",
                        column: x => x.BenevoleID,
                        principalTable: "Benevoles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Centres",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "text", nullable: false),
                    Adresse = table.Column<string>(type: "text", nullable: false),
                    SiegeID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Centres", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Centres_Sieges_SiegeID",
                        column: x => x.SiegeID,
                        principalTable: "Sieges",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Adresse",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BenevoleID = table.Column<int>(type: "integer", nullable: false),
                    DateChangement = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdresseLigne1 = table.Column<string>(type: "text", nullable: false),
                    AdresseLigne2 = table.Column<string>(type: "text", nullable: true),
                    AdresseLigne3 = table.Column<string>(type: "text", nullable: true),
                    CodePostal = table.Column<string>(type: "text", nullable: false),
                    Ville = table.Column<string>(type: "text", nullable: false),
                    CentreID = table.Column<int>(type: "integer", nullable: false),
                    DistanceCentre = table.Column<decimal>(type: "numeric", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adresse", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Adresse_Benevoles_BenevoleID",
                        column: x => x.BenevoleID,
                        principalTable: "Benevoles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Adresse_Centres_CentreID",
                        column: x => x.CentreID,
                        principalTable: "Centres",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Login = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    CentreID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.ID);
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
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BenevoleID = table.Column<int>(type: "integer", nullable: false),
                    AdresseID = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    NbDemiJournees = table.Column<int>(type: "integer", nullable: false),
                    VehiculeID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pointages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Pointages_Adresse_AdresseID",
                        column: x => x.AdresseID,
                        principalTable: "Adresse",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pointages_Benevoles_BenevoleID",
                        column: x => x.BenevoleID,
                        principalTable: "Benevoles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pointages_Vehicule_VehiculeID",
                        column: x => x.VehiculeID,
                        principalTable: "Vehicule",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adresse_BenevoleID_DateChangement",
                table: "Adresse",
                columns: new[] { "BenevoleID", "DateChangement" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Adresse_CentreID",
                table: "Adresse",
                column: "CentreID");

            migrationBuilder.CreateIndex(
                name: "IX_BaremeFiscalDefault_id",
                table: "BaremeFiscalDefault",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_Benevoles_Nom_Prenom",
                table: "Benevoles",
                columns: new[] { "Nom", "Prenom" });

            migrationBuilder.CreateIndex(
                name: "IX_Centres_Nom",
                table: "Centres",
                column: "Nom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Centres_SiegeID",
                table: "Centres",
                column: "SiegeID");

            migrationBuilder.CreateIndex(
                name: "IX_Frais_Annee",
                table: "Frais",
                column: "Annee",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pointages_AdresseID",
                table: "Pointages",
                column: "AdresseID");

            migrationBuilder.CreateIndex(
                name: "IX_Pointages_BenevoleID_Date",
                table: "Pointages",
                columns: new[] { "BenevoleID", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pointages_VehiculeID",
                table: "Pointages",
                column: "VehiculeID");

            migrationBuilder.CreateIndex(
                name: "IX_Sieges_Nom",
                table: "Sieges",
                column: "Nom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_CentreID",
                table: "Utilisateurs",
                column: "CentreID");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Login",
                table: "Utilisateurs",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicule_BenevoleID",
                table: "Vehicule",
                column: "BenevoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicule_id",
                table: "Vehicule",
                column: "id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaremeFiscalDefault");

            migrationBuilder.DropTable(
                name: "BaremeFiscalLignes");

            migrationBuilder.DropTable(
                name: "Frais");

            migrationBuilder.DropTable(
                name: "Pointages");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Adresse");

            migrationBuilder.DropTable(
                name: "Vehicule");

            migrationBuilder.DropTable(
                name: "Centres");

            migrationBuilder.DropTable(
                name: "Benevoles");

            migrationBuilder.DropTable(
                name: "Sieges");
        }
    }
}
