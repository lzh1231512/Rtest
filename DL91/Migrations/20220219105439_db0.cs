using Microsoft.EntityFrameworkCore.Migrations;

namespace DL91.Migrations
{
    public partial class db0 : Migration
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
                    time = table.Column<int>(nullable: false),
                    isLike = table.Column<int>(nullable: false),
                    isVideoDownloaded = table.Column<int>(nullable: false),
                    IsImgDownload = table.Column<int>(nullable: false),
                    videoFileSize = table.Column<long>(nullable: false),
                    typeId = table.Column<int>(nullable: false),
                    isHD = table.Column<bool>(nullable: false)
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
                    isRuning = table.Column<int>(nullable: false),
                    blockKeyWord = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBCfgs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "DBTypes",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(nullable: true),
                    maxID = table.Column<int>(nullable: false),
                    url = table.Column<string>(nullable: true),
                    count = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBTypes", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DB91s");

            migrationBuilder.DropTable(
                name: "DBCfgs");

            migrationBuilder.DropTable(
                name: "DBTypes");
        }
    }
}
