using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdministrareBlocMVC.Data.Migrations
{
    public partial class AddApartamentIdNrPersoaneToAspNetUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApartamentId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NrPersoane",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ApartamentId",
                table: "AspNetUsers",
                column: "ApartamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Apartamente_ApartamentId",
                table: "AspNetUsers",
                column: "ApartamentId",
                principalTable: "Apartamente",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Apartamente_ApartamentId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ApartamentId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApartamentId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NrPersoane",
                table: "AspNetUsers");
        }
    }
}
