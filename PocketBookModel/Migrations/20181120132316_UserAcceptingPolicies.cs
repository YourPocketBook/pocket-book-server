using Microsoft.EntityFrameworkCore.Migrations;

namespace PocketBookServer.Migrations
{
    public partial class UserAcceptingPolicies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UpdateEmailConsentGiven",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateEmailConsentGiven",
                table: "AspNetUsers");
        }
    }
}
