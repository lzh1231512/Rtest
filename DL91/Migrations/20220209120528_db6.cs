using Microsoft.EntityFrameworkCore.Migrations;

namespace DL91.Migrations
{
    public partial class db6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "blockKeyWord",
                table: "DBCfgs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "blockKeyWord",
                table: "DBCfgs");
        }
    }
}
