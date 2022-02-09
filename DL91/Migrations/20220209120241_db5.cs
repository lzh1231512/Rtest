using Microsoft.EntityFrameworkCore.Migrations;

namespace DL91.Migrations
{
    public partial class db5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "videoFileSize",
                table: "DB91s",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "videoFileSize",
                table: "DB91s");
        }
    }
}
