using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KunFarm.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MakeInventorySlotItemOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventorySlots_Items_ItemId",
                table: "InventorySlots");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 1, 21, 48, 30, 54, DateTimeKind.Local).AddTicks(6845),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 1, 17, 55, 52, 369, DateTimeKind.Local).AddTicks(5053));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "FarmStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 1, 14, 48, 30, 54, DateTimeKind.Utc).AddTicks(9622),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 1, 10, 55, 52, 369, DateTimeKind.Utc).AddTicks(7456));

            migrationBuilder.AddForeignKey(
                name: "FK_InventorySlots_Items_ItemId",
                table: "InventorySlots",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventorySlots_Items_ItemId",
                table: "InventorySlots");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 1, 17, 55, 52, 369, DateTimeKind.Local).AddTicks(5053),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 1, 21, 48, 30, 54, DateTimeKind.Local).AddTicks(6845));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "FarmStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 1, 10, 55, 52, 369, DateTimeKind.Utc).AddTicks(7456),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 1, 14, 48, 30, 54, DateTimeKind.Utc).AddTicks(9622));

            migrationBuilder.AddForeignKey(
                name: "FK_InventorySlots_Items_ItemId",
                table: "InventorySlots",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
