using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PocketBookModel.Migrations
{
    public partial class UniqueMedicationName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdviceIfDeclined = table.Column<string>(nullable: false),
                    AdviceIfTaken = table.Column<string>(nullable: false),
                    Dose = table.Column<string>(nullable: false),
                    ExclusionCriteria = table.Column<string>(nullable: false),
                    Form = table.Column<string>(nullable: false),
                    InclusionCriteria = table.Column<string>(nullable: false),
                    Indications = table.Column<string>(nullable: false),
                    LastModified = table.Column<DateTimeOffset>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    PolicyDate = table.Column<DateTime>(nullable: true),
                    Route = table.Column<string>(nullable: false),
                    SideEffects = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medications_Name",
                table: "Medications",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medications");
        }
    }
}
