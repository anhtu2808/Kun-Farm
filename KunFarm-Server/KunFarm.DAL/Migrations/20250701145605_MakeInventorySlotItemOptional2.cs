using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KunFarm.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MakeInventorySlotItemOptional2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 1, 21, 56, 4, 758, DateTimeKind.Local).AddTicks(4226),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 1, 21, 48, 30, 54, DateTimeKind.Local).AddTicks(6845));

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "InventorySlots",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "FarmStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 1, 14, 56, 4, 758, DateTimeKind.Utc).AddTicks(6818),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 1, 14, 48, 30, 54, DateTimeKind.Utc).AddTicks(9622));


            migrationBuilder.CreateTable(
                name: "OnlineShopSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CanBuy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BuyPrice = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineShopSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineShopSlots_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OnlineShopSlots_PlayerStates_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "PlayerStates",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OnlineShopSlots_PlayerStates_SellerId",
                        column: x => x.SellerId,
                        principalTable: "PlayerStates",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
               name: "IX_OnlineShopSlots_BuyerId",
               table: "OnlineShopSlots",
               column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineShopSlots_ItemId",
                table: "OnlineShopSlots",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineShopSlots_SellerId",
                table: "OnlineShopSlots",
                column: "SellerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 1, 21, 48, 30, 54, DateTimeKind.Local).AddTicks(6845),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 1, 21, 56, 4, 758, DateTimeKind.Local).AddTicks(4226));

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "InventorySlots",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "FarmStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 1, 14, 48, 30, 54, DateTimeKind.Utc).AddTicks(9622),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 1, 14, 56, 4, 758, DateTimeKind.Utc).AddTicks(6818));
        }
    }
}
