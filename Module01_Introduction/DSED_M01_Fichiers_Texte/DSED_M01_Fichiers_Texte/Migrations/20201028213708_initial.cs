using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;

namespace DSED_M01_Fichiers_Texte.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Municipalites",
                columns: table => new
                {
                    MunicipaliteId = table.Column<int>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    NomMunicipalite = table.Column<string>(nullable: true),
                    AdresseCourriel = table.Column<string>(nullable: true),
                    AdresseWeb = table.Column<string>(nullable: true),
                    DateProchaineElection = table.Column<DateTime>(nullable: false),
                    Actif = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipalites", x => x.MunicipaliteId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Municipalites");
        }
    }
}
