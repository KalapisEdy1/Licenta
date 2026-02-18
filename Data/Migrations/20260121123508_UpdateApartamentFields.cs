using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdministrareBlocMVC.Data.Migrations
{
    public partial class UpdateApartamentFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrenumeLocatar",
                table: "Apartamente");

            migrationBuilder.AlterColumn<string>(
                name: "NumeLocatar",
                table: "Apartamente",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumarCamere",
                table: "Apartamente",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SuprafataMp",
                table: "Apartamente",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TelefonLocatar",
                table: "Apartamente",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumarCamere",
                table: "Apartamente");

            migrationBuilder.DropColumn(
                name: "SuprafataMp",
                table: "Apartamente");

            migrationBuilder.DropColumn(
                name: "TelefonLocatar",
                table: "Apartamente");

            migrationBuilder.AlterColumn<string>(
                name: "NumeLocatar",
                table: "Apartamente",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrenumeLocatar",
                table: "Apartamente",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
