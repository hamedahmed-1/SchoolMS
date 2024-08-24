using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolMS.Migrations
{
    /// <inheritdoc />
    public partial class eduStag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EducationalStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalStages", x => x.Id);
                });


            migrationBuilder.AddColumn<int>(
               name: "EducationalStageId",
               table: "Grades",
               type: "int",
               nullable: false,
               defaultValue: 0);

            migrationBuilder.AddForeignKey(
            name: "FK_Grades_EducationalStages_EducationalStageId",
            table: "Grades",
            column: "EducationalStageId",
            principalTable: "EducationalStages",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_EducationalStageId",
                table: "Grades",
                column: "EducationalStageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_EducationalStages_EducationalStageId",
                table: "Grades");

            migrationBuilder.DropTable(
                name: "EducationalStages");

            migrationBuilder.DropIndex(
                name: "IX_Grades_EducationalStageId",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "EducationalStageId",
                table: "Grades");
        }
    }
}
