using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PocketBookServer.Migrations
{
    public partial class MedicationPolicyDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PolicyDate",
                table: "Medications",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PolicyDate",
                table: "Medications");
        }
    }
}
