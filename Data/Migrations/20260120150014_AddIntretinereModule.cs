using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdministrareBlocMVC.Data.Migrations
{
    public partial class AddIntretinereModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Intretineri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApartamentId = table.Column<int>(type: "int", nullable: false),
                    Luna = table.Column<int>(type: "int", nullable: false),
                    An = table.Column<int>(type: "int", nullable: false),
                    DataGenerare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Intretineri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Intretineri_Apartamente_ApartamentId",
                        column: x => x.ApartamentId,
                        principalTable: "Apartamente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntretineriDetalii",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntretinereId = table.Column<int>(type: "int", nullable: false),
                    TipCheltuiala = table.Column<int>(type: "int", nullable: false),
                    DenumireCustom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Suma = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntretineriDetalii", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntretineriDetalii_Intretineri_IntretinereId",
                        column: x => x.IntretinereId,
                        principalTable: "Intretineri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Intretineri_ApartamentId",
                table: "Intretineri",
                column: "ApartamentId");

            migrationBuilder.CreateIndex(
                name: "IX_IntretineriDetalii_IntretinereId",
                table: "IntretineriDetalii",
                column: "IntretinereId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntretineriDetalii");

            migrationBuilder.DropTable(
                name: "Intretineri");
        }
    }
}
