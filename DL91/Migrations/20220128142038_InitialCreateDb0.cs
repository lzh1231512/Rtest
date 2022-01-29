using Microsoft.EntityFrameworkCore.Migrations;

namespace DL91.Migrations
{
    public partial class InitialCreateDb0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DB91s",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(nullable: true),
                    url = table.Column<string>(nullable: true),
                    imgUrl = table.Column<string>(nullable: true),
                    time = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DB91s", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "DBCfgs",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    maxID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBCfgs", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DB91s");

            migrationBuilder.DropTable(
                name: "DBCfgs");
        }
    }
}
