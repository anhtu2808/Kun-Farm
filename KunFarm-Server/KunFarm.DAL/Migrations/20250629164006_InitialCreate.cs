using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KunFarm.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 6, 29, 23, 40, 6, 676, DateTimeKind.Local).AddTicks(3120),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 6, 25, 13, 35, 44, 735, DateTimeKind.Local).AddTicks(5250));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 6, 25, 13, 35, 44, 735, DateTimeKind.Local).AddTicks(5250),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 6, 29, 23, 40, 6, 676, DateTimeKind.Local).AddTicks(3120));
        }
    }
}
