using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KunFarm.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddChickensAndEggsState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerToolbars",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 7, 11, 5, 51, 978, DateTimeKind.Local).AddTicks(3390),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 3, 5, 6, 11, 719, DateTimeKind.Local).AddTicks(340));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 7, 11, 5, 51, 978, DateTimeKind.Local).AddTicks(880),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 3, 5, 6, 11, 718, DateTimeKind.Local).AddTicks(4810));

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
                oldDefaultValue: new DateTime(2025, 7, 2, 22, 6, 11, 720, DateTimeKind.Utc).AddTicks(430));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                defaultValue: new DateTime(2025, 7, 3, 5, 6, 11, 719, DateTimeKind.Local).AddTicks(340),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 7, 11, 5, 51, 978, DateTimeKind.Local).AddTicks(3390));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 3, 5, 6, 11, 718, DateTimeKind.Local).AddTicks(4810),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 7, 11, 5, 51, 978, DateTimeKind.Local).AddTicks(880));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "FarmStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 2, 22, 6, 11, 720, DateTimeKind.Utc).AddTicks(430),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 7, 4, 5, 51, 978, DateTimeKind.Utc).AddTicks(3900));
        }
    }
}
