using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGUI_Stage2.Migrations
{
    /// <inheritdoc />
    public partial class wowowowowo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_AspNetUsers_DoctorId",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitSlots_Schedule_ParentScheduleId",
                table: "VisitSlots");

            migrationBuilder.DropIndex(
                name: "IX_VisitSlots_ParentScheduleId",
                table: "VisitSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Schedule",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "ParentScheduleId",
                table: "VisitSlots");

            migrationBuilder.RenameTable(
                name: "Schedule",
                newName: "ScheduleEntries");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "ScheduleEntries",
                newName: "ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedule_DoctorId",
                table: "ScheduleEntries",
                newName: "IX_ScheduleEntries_ScheduleId");

            migrationBuilder.AddColumn<string>(
                name: "ParentScheduleEntryId",
                table: "VisitSlots",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ScheduleEntries",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduleEntries",
                table: "ScheduleEntries",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    DateOfMonday = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DoctorId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_VisitSlots_ParentScheduleEntryId",
                table: "VisitSlots",
                column: "ParentScheduleEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DoctorId",
                table: "Schedules",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEntries_Schedules_ScheduleId",
                table: "ScheduleEntries",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSlots_ScheduleEntries_ParentScheduleEntryId",
                table: "VisitSlots",
                column: "ParentScheduleEntryId",
                principalTable: "ScheduleEntries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEntries_Schedules_ScheduleId",
                table: "ScheduleEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitSlots_ScheduleEntries_ParentScheduleEntryId",
                table: "VisitSlots");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_VisitSlots_ParentScheduleEntryId",
                table: "VisitSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduleEntries",
                table: "ScheduleEntries");

            migrationBuilder.DropColumn(
                name: "ParentScheduleEntryId",
                table: "VisitSlots");

            migrationBuilder.RenameTable(
                name: "ScheduleEntries",
                newName: "Schedule");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "Schedule",
                newName: "DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleEntries_ScheduleId",
                table: "Schedule",
                newName: "IX_Schedule_DoctorId");

            migrationBuilder.AddColumn<int>(
                name: "ParentScheduleId",
                table: "VisitSlots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Schedule",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Schedule",
                table: "Schedule",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_VisitSlots_ParentScheduleId",
                table: "VisitSlots",
                column: "ParentScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_AspNetUsers_DoctorId",
                table: "Schedule",
                column: "DoctorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSlots_Schedule_ParentScheduleId",
                table: "VisitSlots",
                column: "ParentScheduleId",
                principalTable: "Schedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
