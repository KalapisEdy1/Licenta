using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdministrareBlocMVC.Data.Migrations
{
    public partial class AddPublicareCheltuieliLunare : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublicariCheltuieliLunare",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlocId = table.Column<int>(type: "int", nullable: false),
                    Scara = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Luna = table.Column<int>(type: "int", nullable: false),
                    An = table.Column<int>(type: "int", nullable: false),
                    DataPublicare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataColectare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicariCheltuieliLunare", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicariCheltuieliLunare_Blocuri_BlocId",
                        column: x => x.BlocId,
                        principalTable: "Blocuri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicariCheltuieliLunare_BlocId",
                table: "PublicariCheltuieliLunare",
                column: "BlocId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicariCheltuieliLunare");
        }
    }
}
