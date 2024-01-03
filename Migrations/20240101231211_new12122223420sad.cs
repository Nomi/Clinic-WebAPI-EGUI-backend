using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGUI_Stage2.Migrations
{
    /// <inheritdoc />
    public partial class new12122223420sad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisitSlots_AspNetUsers_PatientId",
                table: "VisitSlots");

            migrationBuilder.AlterColumn<string>(
                name: "PatientId",
                table: "VisitSlots",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSlots_AspNetUsers_PatientId",
                table: "VisitSlots",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisitSlots_AspNetUsers_PatientId",
                table: "VisitSlots");

            migrationBuilder.AlterColumn<string>(
                name: "PatientId",
                table: "VisitSlots",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitSlots_AspNetUsers_PatientId",
                table: "VisitSlots",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
