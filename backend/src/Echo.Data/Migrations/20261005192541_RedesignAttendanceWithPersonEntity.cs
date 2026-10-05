using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class RedesignAttendanceWithPersonEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_AttendanceContexts_AttendanceContextId",
                table: "AttendanceRecords"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Congregations_CongregationId",
                table: "AttendanceRecords"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Members_MemberId",
                table: "AttendanceRecords"
            );

            migrationBuilder.DropTable(name: "AttendanceContexts");

            migrationBuilder.DropIndex(name: "IX_Members_Name", table: "Members");

            migrationBuilder.DropIndex(name: "IX_Members_Name_Id", table: "Members");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttendanceRecords",
                table: "AttendanceRecords"
            );

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_CongregationId",
                table: "AttendanceRecords"
            );

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_ForDate",
                table: "AttendanceRecords"
            );

            migrationBuilder.DropColumn(name: "Name", table: "Members");

            migrationBuilder.DropColumn(name: "EmailAddress", table: "Members");

            migrationBuilder.DropColumn(name: "FirstName", table: "Members");

            migrationBuilder.DropColumn(name: "LastName", table: "Members");

            migrationBuilder.DropColumn(name: "OtherNames", table: "Members");

            migrationBuilder.DropColumn(name: "PhoneNumber", table: "Members");

            migrationBuilder.DropColumn(name: "AttendeeType", table: "AttendanceRecords");

            migrationBuilder.RenameTable(name: "AttendanceRecords", newName: "Attendance");

            migrationBuilder.RenameColumn(name: "Id", table: "Members", newName: "PersonId");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Attendance",
                newName: "PersonId"
            );

            migrationBuilder.RenameColumn(name: "ForDate", table: "Attendance", newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Attendance",
                newName: "Notes"
            );

            migrationBuilder.RenameColumn(
                name: "AttendanceContextId",
                table: "Attendance",
                newName: "AttendanceTypeId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_MemberId",
                table: "Attendance",
                newName: "IX_Attendance_PersonId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_ForDate_Id",
                table: "Attendance",
                newName: "IX_Attendance_Date_Id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_AttendanceContextId",
                table: "Attendance",
                newName: "IX_Attendance_AttendanceTypeId"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attendance",
                table: "Attendance",
                column: "Id"
            );

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CongregationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false,
                        computedColumnSql: "TRIM(COALESCE(\"LastName\", '') || ' ' || COALESCE(\"FirstName\", '') || ' ' || COALESCE(\"OtherNames\", ''))",
                        stored: true
                    ),
                    FirstName = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    LastName = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    OtherNames = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                    PhoneNumber = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    EmailAddress = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    DeletedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                    table.ForeignKey(
                        name: "FK_People_Congregations_CongregationId",
                        column: x => x.CongregationId,
                        principalTable: "Congregations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Visitors",
                columns: table => new
                {
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    CongregationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                    ConvertedToMemberPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConvertedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    DeletedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitors", x => x.PersonId);
                    table.ForeignKey(
                        name: "FK_Visitors_Congregations_CongregationId",
                        column: x => x.CongregationId,
                        principalTable: "Congregations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Visitors_Members_ConvertedToMemberPersonId",
                        column: x => x.ConvertedToMemberPersonId,
                        principalTable: "Members",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Visitors_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Members_Status",
                table: "Members",
                column: "Status"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_CongregationId_AttendanceTypeId_Date",
                table: "Attendance",
                columns: new[] { "CongregationId", "AttendanceTypeId", "Date" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_CongregationId_PersonId",
                table: "Attendance",
                columns: new[] { "CongregationId", "PersonId" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_CongregationId_PersonId_AttendanceTypeId_Date",
                table: "Attendance",
                columns: new[] { "CongregationId", "PersonId", "AttendanceTypeId", "Date" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_People_CongregationId",
                table: "People",
                column: "CongregationId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_People_CongregationId_Kind",
                table: "People",
                columns: new[] { "CongregationId", "Kind" }
            );

            migrationBuilder
                .CreateIndex(name: "IX_People_Name", table: "People", column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_People_Name_Id",
                table: "People",
                columns: new[] { "Name", "Id" },
                filter: "\"DeletedAt\" IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_CongregationId",
                table: "Visitors",
                column: "CongregationId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_ConvertedToMemberPersonId",
                table: "Visitors",
                column: "ConvertedToMemberPersonId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_AttendanceTypes_AttendanceTypeId",
                table: "Attendance",
                column: "AttendanceTypeId",
                principalTable: "AttendanceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Congregations_CongregationId",
                table: "Attendance",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_People_PersonId",
                table: "Attendance",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Members_People_PersonId",
                table: "Members",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_AttendanceTypes_AttendanceTypeId",
                table: "Attendance"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Congregations_CongregationId",
                table: "Attendance"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_People_PersonId",
                table: "Attendance"
            );

            migrationBuilder.DropForeignKey(name: "FK_Members_People_PersonId", table: "Members");

            migrationBuilder.DropTable(name: "Visitors");

            migrationBuilder.DropTable(name: "People");

            migrationBuilder.DropIndex(name: "IX_Members_Status", table: "Members");

            migrationBuilder.DropPrimaryKey(name: "PK_Attendance", table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_CongregationId_AttendanceTypeId_Date",
                table: "Attendance"
            );

            migrationBuilder.DropIndex(
                name: "IX_Attendance_CongregationId_PersonId",
                table: "Attendance"
            );

            migrationBuilder.DropIndex(
                name: "IX_Attendance_CongregationId_PersonId_AttendanceTypeId_Date",
                table: "Attendance"
            );

            migrationBuilder.RenameTable(name: "Attendance", newName: "AttendanceRecords");

            migrationBuilder.RenameColumn(name: "PersonId", table: "Members", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "AttendanceRecords",
                newName: "MemberId"
            );

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "AttendanceRecords",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "AttendanceRecords",
                newName: "ForDate"
            );

            migrationBuilder.RenameColumn(
                name: "AttendanceTypeId",
                table: "AttendanceRecords",
                newName: "AttendanceContextId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Attendance_PersonId",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_MemberId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Attendance_Date_Id",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_ForDate_Id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Attendance_AttendanceTypeId",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_AttendanceContextId"
            );

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "Members",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Members",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Members",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "OtherNames",
                table: "Members",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Members",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "AttendeeType",
                table: "AttendanceRecords",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Members",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                computedColumnSql: "TRIM(COALESCE(\"LastName\", '') || ' ' || COALESCE(\"FirstName\", '') || ' ' || COALESCE(\"OtherNames\", ''))",
                stored: true
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttendanceRecords",
                table: "AttendanceRecords",
                column: "Id"
            );

            migrationBuilder.CreateTable(
                name: "AttendanceContexts",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    AttendanceTypeId = table.Column<int>(type: "integer", nullable: false),
                    CongregationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    DeletedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    Name = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceContexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceContexts_AttendanceTypes_AttendanceTypeId",
                        column: x => x.AttendanceTypeId,
                        principalTable: "AttendanceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_AttendanceContexts_Congregations_CongregationId",
                        column: x => x.CongregationId,
                        principalTable: "Congregations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder
                .CreateIndex(name: "IX_Members_Name", table: "Members", column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Members_Name_Id",
                table: "Members",
                columns: new[] { "Name", "Id" },
                filter: "\"DeletedAt\" IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_CongregationId",
                table: "AttendanceRecords",
                column: "CongregationId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_ForDate",
                table: "AttendanceRecords",
                column: "ForDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceContexts_AttendanceTypeId_Name",
                table: "AttendanceContexts",
                columns: new[] { "AttendanceTypeId", "Name" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceContexts_CongregationId",
                table: "AttendanceContexts",
                column: "CongregationId"
            );

            migrationBuilder
                .CreateIndex(
                    name: "IX_AttendanceContexts_Name",
                    table: "AttendanceContexts",
                    column: "Name"
                )
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_AttendanceContexts_AttendanceContextId",
                table: "AttendanceRecords",
                column: "AttendanceContextId",
                principalTable: "AttendanceContexts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Congregations_CongregationId",
                table: "AttendanceRecords",
                column: "CongregationId",
                principalTable: "Congregations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Members_MemberId",
                table: "AttendanceRecords",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull
            );
        }
    }
}
