using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReportDemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExamPromotionAlumniModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alumni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RollNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GuardianName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GuardianContact = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ProfileImage = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OriginalStudentId = table.Column<int>(type: "integer", nullable: false),
                    GraduatedFromClassId = table.Column<int>(type: "integer", nullable: false),
                    GraduationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AcademicYear = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FinalPercentage = table.Column<double>(type: "double precision", nullable: true),
                    FinalGrade = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    GraduationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CurrentOccupation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CurrentEmployer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HigherEducation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CurrentEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CurrentPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CurrentAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EnrollmentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alumni_Classes_GraduatedFromClassId",
                        column: x => x.GraduatedFromClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    Term = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AcademicYear = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Percentage = table.Column<double>(type: "double precision", nullable: false),
                    Grade = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: false),
                    ExamCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    ExamDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConductedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamResults_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamResults_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PromotionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    OldClassId = table.Column<int>(type: "integer", nullable: false),
                    NewClassId = table.Column<int>(type: "integer", nullable: true),
                    PromotionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AcademicYear = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PromotionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PromotedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FinalPercentage = table.Column<double>(type: "double precision", nullable: true),
                    FinalGrade = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    IsPromoted = table.Column<bool>(type: "boolean", nullable: false),
                    IsGraduated = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionHistories_Classes_NewClassId",
                        column: x => x.NewClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionHistories_Classes_OldClassId",
                        column: x => x.OldClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionHistories_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_Email",
                table: "Alumni",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_GraduatedFromClassId",
                table: "Alumni",
                column: "GraduatedFromClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_OriginalStudentId",
                table: "Alumni",
                column: "OriginalStudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_ClassId",
                table: "ExamResults",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_Student_Term_Year",
                table: "ExamResults",
                columns: new[] { "StudentId", "Term", "AcademicYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionHistories_NewClassId",
                table: "PromotionHistories",
                column: "NewClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionHistories_OldClassId",
                table: "PromotionHistories",
                column: "OldClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionHistories_StudentId",
                table: "PromotionHistories",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alumni");

            migrationBuilder.DropTable(
                name: "ExamResults");

            migrationBuilder.DropTable(
                name: "PromotionHistories");
        }
    }
}
