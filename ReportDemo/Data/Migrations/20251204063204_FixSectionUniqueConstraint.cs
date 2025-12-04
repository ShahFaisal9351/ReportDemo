using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportDemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSectionUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sections_ClassId",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Sections_Name",
                table: "Sections");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ClassId_Name",
                table: "Sections",
                columns: new[] { "ClassId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sections_ClassId_Name",
                table: "Sections");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ClassId",
                table: "Sections",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_Name",
                table: "Sections",
                column: "Name",
                unique: true);
        }
    }
}
