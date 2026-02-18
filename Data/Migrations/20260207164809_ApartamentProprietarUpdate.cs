using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdministrareBlocMVC.Data.Migrations
{
    public partial class ApartamentProprietarUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipLocatar",
                table: "Apartamente");

            migrationBuilder.RenameColumn(
                name: "TelefonLocatar",
                table: "Apartamente",
                newName: "TelefonProprietar");

            migrationBuilder.RenameColumn(
                name: "NumeLocatar",
                table: "Apartamente",
                newName: "NumeProprietar");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TelefonProprietar",
                table: "Apartamente",
                newName: "TelefonLocatar");

            migrationBuilder.RenameColumn(
                name: "NumeProprietar",
                table: "Apartamente",
                newName: "NumeLocatar");

            migrationBuilder.AddColumn<int>(
                name: "TipLocatar",
                table: "Apartamente",
                type: "int",
                nullable: true);
        }
    }
}
