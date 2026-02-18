using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdministrareBlocMVC.Data.Migrations
{
    public partial class RemoveDenumireCustomFromIntretinereDetalii : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DenumireCustom",
                table: "IntretineriDetalii");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DenumireCustom",
                table: "IntretineriDetalii",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
