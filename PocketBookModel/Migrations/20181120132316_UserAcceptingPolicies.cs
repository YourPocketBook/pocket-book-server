using Microsoft.EntityFrameworkCore.Migrations;

namespace PocketBookModel.Migrations
{
    public partial class UserAcceptingPolicies : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateEmailConsentGiven",
                table: "AspNetUsers");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UpdateEmailConsentGiven",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }
    }
}
