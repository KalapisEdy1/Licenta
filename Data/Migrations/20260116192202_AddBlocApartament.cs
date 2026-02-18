using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdministrareBlocMVC.Data.Migrations
{
    public partial class AddBlocApartament : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blocuri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nume = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adresa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocuri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Apartamente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numar = table.Column<int>(type: "int", nullable: false),
                    Etaj = table.Column<int>(type: "int", nullable: false),
                    BlocId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apartamente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Apartamente_Blocuri_BlocId",
                        column: x => x.BlocId,
                        principalTable: "Blocuri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apartamente_BlocId",
                table: "Apartamente",
                column: "BlocId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Apartamente");

            migrationBuilder.DropTable(
                name: "Blocuri");
        }
    }
}
