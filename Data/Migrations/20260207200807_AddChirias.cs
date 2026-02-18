using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdministrareBlocMVC.Data.Migrations
{
    public partial class AddChirias : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chiriasi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApartamentId = table.Column<int>(type: "int", nullable: false),
                    NumeChirias = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TelefonChirias = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chiriasi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chiriasi_Apartamente_ApartamentId",
                        column: x => x.ApartamentId,
                        principalTable: "Apartamente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chiriasi_ApartamentId",
                table: "Chiriasi",
                column: "ApartamentId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chiriasi");
        }
    }
}
