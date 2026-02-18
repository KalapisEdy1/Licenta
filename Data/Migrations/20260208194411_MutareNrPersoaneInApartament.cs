using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdministrareBlocMVC.Data.Migrations
{
    public partial class MutareNrPersoaneInApartament : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NrPersoane",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "NrPersoane",
                table: "Apartamente",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NrPersoane",
                table: "Apartamente");

            migrationBuilder.AddColumn<int>(
                name: "NrPersoane",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
