using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdministrareBlocMVC.Data.Migrations
{
    public partial class AddLocatarInfoToApartament : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumeLocatar",
                table: "Apartamente",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrenumeLocatar",
                table: "Apartamente",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipLocatar",
                table: "Apartamente",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeLocatar",
                table: "Apartamente");

            migrationBuilder.DropColumn(
                name: "PrenumeLocatar",
                table: "Apartamente");

            migrationBuilder.DropColumn(
                name: "TipLocatar",
                table: "Apartamente");
        }
    }
}
