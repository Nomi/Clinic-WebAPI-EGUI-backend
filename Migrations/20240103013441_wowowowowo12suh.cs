using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGUI_Stage2.Migrations
{
    /// <inheritdoc />
    public partial class wowowowowo12suh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEntries_Schedules_ScheduleId",
                table: "ScheduleEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_AspNetUsers_DoctorId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitSlots_ScheduleEntries_ParentScheduleEntryId",
                table: "VisitSlots");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEntries_Schedules_ScheduleId",
                table: "ScheduleEntries",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_AspNetUsers_DoctorId",
                table: "Schedules",
                column: "DoctorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSlots_ScheduleEntries_ParentScheduleEntryId",
                table: "VisitSlots",
                column: "ParentScheduleEntryId",
                principalTable: "ScheduleEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEntries_Schedules_ScheduleId",
                table: "ScheduleEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_AspNetUsers_DoctorId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitSlots_ScheduleEntries_ParentScheduleEntryId",
                table: "VisitSlots");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEntries_Schedules_ScheduleId",
                table: "ScheduleEntries",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_AspNetUsers_DoctorId",
                table: "Schedules",
                column: "DoctorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSlots_ScheduleEntries_ParentScheduleEntryId",
                table: "VisitSlots",
                column: "ParentScheduleEntryId",
                principalTable: "ScheduleEntries",
                principalColumn: "Id");
        }
    }
}
