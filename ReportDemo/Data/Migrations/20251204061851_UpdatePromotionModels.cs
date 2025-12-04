using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReportDemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePromotionModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Classes_ClassName_Section",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "Section",
                table: "Students");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "Students",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "Students",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NewSessionId",
                table: "PromotionHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PromotionHistories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldSessionId",
                table: "PromotionHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "ExamResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Classes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AcademicYear = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_SectionId",
                table: "Students",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SessionId",
                table: "Students",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionHistories_NewSessionId",
                table: "PromotionHistories",
                column: "NewSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionHistories_OldSessionId",
                table: "PromotionHistories",
                column: "OldSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_SessionId",
                table: "ExamResults",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ClassId",
                table: "Sections",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_Name",
                table: "Sections",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_AcademicYear",
                table: "Sessions",
                column: "AcademicYear");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_IsCurrent",
                table: "Sessions",
                column: "IsCurrent");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_Name",
                table: "Sessions",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_Sessions_SessionId",
                table: "ExamResults",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionHistories_Sessions_NewSessionId",
                table: "PromotionHistories",
                column: "NewSessionId",
                principalTable: "Sessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionHistories_Sessions_OldSessionId",
                table: "PromotionHistories",
                column: "OldSessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Sections_SectionId",
                table: "Students",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Sessions_SessionId",
                table: "Students",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_Sessions_SessionId",
                table: "ExamResults");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionHistories_Sessions_NewSessionId",
                table: "PromotionHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionHistories_Sessions_OldSessionId",
                table: "PromotionHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Sections_SectionId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Sessions_SessionId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Students_SectionId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_SessionId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_PromotionHistories_NewSessionId",
                table: "PromotionHistories");

            migrationBuilder.DropIndex(
                name: "IX_PromotionHistories_OldSessionId",
                table: "PromotionHistories");

            migrationBuilder.DropIndex(
                name: "IX_ExamResults_SessionId",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "NewSessionId",
                table: "PromotionHistories");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PromotionHistories");

            migrationBuilder.DropColumn(
                name: "OldSessionId",
                table: "PromotionHistories");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Classes");

            migrationBuilder.AddColumn<string>(
                name: "Section",
                table: "Students",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_ClassName_Section",
                table: "Classes",
                columns: new[] { "ClassName", "Section" },
                unique: true);
        }
    }
}
