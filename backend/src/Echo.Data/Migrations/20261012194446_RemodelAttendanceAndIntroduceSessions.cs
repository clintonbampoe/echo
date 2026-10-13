using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemodelAttendanceAndIntroduceSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_AttendanceTypes_AttendanceTypeId",
                table: "Attendance");

            migrationBuilder.DropTable(
                name: "AttendanceTypes");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_AttendanceTypeId",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_CongregationId_AttendanceTypeId_Date",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_CongregationId_PersonId",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_CongregationId_PersonId_AttendanceTypeId_Date",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_Date_Id",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "AttendanceTypeId",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Attendance");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInAt",
                table: "Attendance",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonKindAtTime",
                table: "Attendance",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordedBy",
                table: "Attendance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "Attendance",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "SessionLabels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CongregationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionLabels_Congregations_CongregationId",
                        column: x => x.CongregationId,
                        principalTable: "Congregations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CongregationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionLabelId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Topic = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartsAt = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndsAt = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Congregations_CongregationId",
                        column: x => x.CongregationId,
                        principalTable: "Congregations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_SessionLabels_SessionLabelId",
                        column: x => x.SessionLabelId,
                        principalTable: "SessionLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_CongregationId",
                table: "Attendance",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_CongregationId_PersonKindAtTime",
                table: "Attendance",
                columns: new[] { "CongregationId", "PersonKindAtTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_RecordedBy",
                table: "Attendance",
                column: "RecordedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_SessionId",
                table: "Attendance",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_SessionId_PersonId",
                table: "Attendance",
                columns: new[] { "SessionId", "PersonId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SessionLabels_CongregationId",
                table: "SessionLabels",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionLabels_CongregationId_Name",
                table: "SessionLabels",
                columns: new[] { "CongregationId", "Name" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CongregationId_Date",
                table: "Sessions",
                columns: new[] { "CongregationId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CreatedBy",
                table: "Sessions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionLabelId",
                table: "Sessions",
                column: "SessionLabelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Sessions_SessionId",
                table: "Attendance",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Users_RecordedBy",
                table: "Attendance",
                column: "RecordedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Sessions_SessionId",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Users_RecordedBy",
                table: "Attendance");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "SessionLabels");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_CongregationId",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_CongregationId_PersonKindAtTime",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_RecordedBy",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_SessionId",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_SessionId_PersonId",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "CheckedInAt",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "PersonKindAtTime",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "RecordedBy",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Attendance");

            migrationBuilder.AddColumn<int>(
                name: "AttendanceTypeId",
                table: "Attendance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "CheckInTime",
                table: "Attendance",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "Attendance",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Attendance",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttendanceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CongregationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceTypes_Congregations_CongregationId",
                        column: x => x.CongregationId,
                        principalTable: "Congregations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_AttendanceTypeId",
                table: "Attendance",
                column: "AttendanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_CongregationId_AttendanceTypeId_Date",
                table: "Attendance",
                columns: new[] { "CongregationId", "AttendanceTypeId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_CongregationId_PersonId",
                table: "Attendance",
                columns: new[] { "CongregationId", "PersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_CongregationId_PersonId_AttendanceTypeId_Date",
                table: "Attendance",
                columns: new[] { "CongregationId", "PersonId", "AttendanceTypeId", "Date" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Date_Id",
                table: "Attendance",
                columns: new[] { "Date", "Id" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceTypes_CongregationId",
                table: "AttendanceTypes",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceTypes_CongregationId_Name",
                table: "AttendanceTypes",
                columns: new[] { "CongregationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceTypes_Name",
                table: "AttendanceTypes",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_AttendanceTypes_AttendanceTypeId",
                table: "Attendance",
                column: "AttendanceTypeId",
                principalTable: "AttendanceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
