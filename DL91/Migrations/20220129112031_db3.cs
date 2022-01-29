using Microsoft.EntityFrameworkCore.Migrations;

namespace DL91.Migrations
{
    public partial class db3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "isRuning",
                table: "DBCfgs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IsImgDownload",
                table: "DB91s",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isRuning",
                table: "DBCfgs");

            migrationBuilder.DropColumn(
                name: "IsImgDownload",
                table: "DB91s");
        }
    }
}
