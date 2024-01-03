using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGUI_Stage2.Migrations
{
    /// <inheritdoc />
    public partial class new121 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisitSlots_AspNetUsers_doctorId",
                table: "VisitSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitSlots_Schedule_ScheduleEntryId",
                table: "VisitSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitSlots_Schedule_ScheduleEntryId1",
                table: "VisitSlots");

            migrationBuilder.DropIndex(
                name: "IX_VisitSlots_doctorId",
                table: "VisitSlots");

            migrationBuilder.DropIndex(
                name: "IX_VisitSlots_ScheduleEntryId",
                table: "VisitSlots");

            migrationBuilder.DropIndex(
                name: "IX_VisitSlots_ScheduleEntryId1",
                table: "VisitSlots");

            migrationBuilder.DropColumn(
                name: "ScheduleEntryId",
                table: "VisitSlots");

            migrationBuilder.DropColumn(
                name: "ScheduleEntryId1",
                table: "VisitSlots");

            migrationBuilder.DropColumn(
                name: "doctorId",
                table: "VisitSlots");

            migrationBuilder.RenameColumn(
                name: "DoctorDate",
                table: "Schedule",
                newName: "Date");

            migrationBuilder.AddColumn<int>(
                name: "ParentScheduleId",
                table: "VisitSlots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUserRoles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoleId1",
                table: "AspNetUserRoles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "AspNetUserRoles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetRoles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_VisitSlots_ParentScheduleId",
                table: "VisitSlots",
                column: "ParentScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId1",
                table: "AspNetUserRoles",
                column: "RoleId1");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId1",
                table: "AspNetUserRoles",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId1",
                table: "AspNetUserRoles",
                column: "RoleId1",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId1",
                table: "AspNetUserRoles",
                column: "UserId1",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId1",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId1",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_VisitSlots_Schedule_ParentScheduleId",
                table: "VisitSlots");

            migrationBuilder.DropIndex(
                name: "IX_VisitSlots_ParentScheduleId",
                table: "VisitSlots");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_RoleId1",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UserId1",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "ParentScheduleId",
                table: "VisitSlots");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "RoleId1",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetRoles");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Schedule",
                newName: "DoctorDate");

            migrationBuilder.AddColumn<int>(
                name: "ScheduleEntryId",
                table: "VisitSlots",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleEntryId1",
                table: "VisitSlots",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "doctorId",
                table: "VisitSlots",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_VisitSlots_doctorId",
                table: "VisitSlots",
                column: "doctorId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitSlots_ScheduleEntryId",
                table: "VisitSlots",
                column: "ScheduleEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitSlots_ScheduleEntryId1",
                table: "VisitSlots",
                column: "ScheduleEntryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSlots_AspNetUsers_doctorId",
                table: "VisitSlots",
                column: "doctorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSlots_Schedule_ScheduleEntryId",
                table: "VisitSlots",
                column: "ScheduleEntryId",
                principalTable: "Schedule",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSlots_Schedule_ScheduleEntryId1",
                table: "VisitSlots",
                column: "ScheduleEntryId1",
                principalTable: "Schedule",
                principalColumn: "Id");
        }
    }
}
