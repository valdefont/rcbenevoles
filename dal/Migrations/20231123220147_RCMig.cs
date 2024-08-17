using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace dal.Migrations
{
    /// <inheritdoc />
    public partial class RCMig : Migration
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
                    //table.PrimaryKey("PK_BaremeFiscalDefault", x => x.id);
                });


            InsertBareme(migrationBuilder, 1, 3, 5000);
            InsertBareme(migrationBuilder, 2, 4, 5000);
            InsertBareme(migrationBuilder, 3, 5, 5000);
            InsertBareme(migrationBuilder, 4, 6, 5000);
            InsertBareme(migrationBuilder, 5, 7, 5000);
            InsertBareme(migrationBuilder, 6, 3, 20000);
            InsertBareme(migrationBuilder, 7, 4, 20000);
            InsertBareme(migrationBuilder, 8, 5, 20000);
            InsertBareme(migrationBuilder, 9, 6, 20000);
            InsertBareme(migrationBuilder, 10, 7, 20000);

        }

        private void InsertBareme(MigrationBuilder migrationBuilder, int id, int nbChevaux, int limiteKm)
        {
            migrationBuilder.InsertData("BaremeFiscalDefault",
                new string[] { "id", "NbChevaux", "LimiteKm" },
                new object[] { id, nbChevaux, limiteKm });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaremeFiscalDefault");

         
        }
    }
}
