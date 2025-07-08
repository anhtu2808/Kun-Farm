using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KunFarm.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MoveChickensEggsToFarmState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChickensStateJson",
                table: "PlayerStates");

            migrationBuilder.DropColumn(
                name: "EggsStateJson",
                table: "PlayerStates");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerToolbars",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 7, 11, 15, 20, 925, DateTimeKind.Local).AddTicks(6010),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 7, 11, 5, 51, 978, DateTimeKind.Local).AddTicks(3390));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 7, 11, 15, 20, 925, DateTimeKind.Local).AddTicks(3130),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 7, 11, 5, 51, 978, DateTimeKind.Local).AddTicks(880));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "FarmStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 7, 4, 15, 20, 925, DateTimeKind.Utc).AddTicks(6560),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 7, 4, 5, 51, 978, DateTimeKind.Utc).AddTicks(3900));

            migrationBuilder.AddColumn<string>(
                name: "ChickensStateJson",
                table: "FarmStates",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EggsStateJson",
                table: "FarmStates",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChickensStateJson",
                table: "FarmStates");

            migrationBuilder.DropColumn(
                name: "EggsStateJson",
                table: "FarmStates");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerToolbars",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 7, 11, 5, 51, 978, DateTimeKind.Local).AddTicks(3390),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 7, 11, 15, 20, 925, DateTimeKind.Local).AddTicks(6010));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 7, 11, 5, 51, 978, DateTimeKind.Local).AddTicks(880),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 7, 11, 15, 20, 925, DateTimeKind.Local).AddTicks(3130));

            migrationBuilder.AddColumn<string>(
                name: "ChickensStateJson",
                table: "PlayerStates",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EggsStateJson",
                table: "PlayerStates",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "FarmStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 7, 4, 5, 51, 978, DateTimeKind.Utc).AddTicks(3900),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 7, 4, 15, 20, 925, DateTimeKind.Utc).AddTicks(6560));
        }
    }
}
