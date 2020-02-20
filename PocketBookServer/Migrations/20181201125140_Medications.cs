using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PocketBookServer.Migrations
{
    public partial class Medications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    AdviceIfDeclined = table.Column<string>(nullable: false),
                    AdviceIfTaken = table.Column<string>(nullable: false),
                    Dose = table.Column<string>(nullable: false),
                    ExclusionCriteria = table.Column<string>(nullable: false),
                    Form = table.Column<string>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    InclusionCriteria = table.Column<string>(nullable: false),
                    Indications = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Route = table.Column<string>(nullable: false),
                    SideEffects = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medications");
        }
    }
}
