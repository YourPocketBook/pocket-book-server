using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace PocketBookModel.Migrations
{
    public partial class MedicationPolicyDate : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PolicyDate",
                table: "Medications");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PolicyDate",
                table: "Medications",
                nullable: true);
        }
    }
}
