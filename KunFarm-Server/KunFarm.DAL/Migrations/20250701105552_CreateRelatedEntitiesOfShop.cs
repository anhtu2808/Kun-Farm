using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KunFarm.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CreateRelatedEntitiesOfShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RegularShopSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CanBuy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BuyPrice = table.Column<int>(type: "int", nullable: false),
                    StockLimit = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegularShopSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegularShopSlots_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventorySlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SlotIndex = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PlayerStateId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventorySlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventorySlots_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventorySlots_PlayerStates_PlayerStateId",
                        column: x => x.PlayerStateId,
                        principalTable: "PlayerStates",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.CreateTable(
                name: "PlayerRegularShopSlots",
                columns: table => new
                {
                    PlayerStateId = table.Column<int>(type: "int", nullable: false),
                    RegularShopSlotId = table.Column<int>(type: "int", nullable: false),
                    CurrentStock = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRegularShopSlots", x => new { x.PlayerStateId, x.RegularShopSlotId });
                    table.ForeignKey(
                        name: "FK_PlayerRegularShopSlots_PlayerStates_PlayerStateId",
                        column: x => x.PlayerStateId,
                        principalTable: "PlayerStates",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerRegularShopSlots_RegularShopSlots_RegularShopSlotId",
                        column: x => x.RegularShopSlotId,
                        principalTable: "RegularShopSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InventorySlots_ItemId",
                table: "InventorySlots",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventorySlots_PlayerStateId",
                table: "InventorySlots",
                column: "PlayerStateId");

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

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRegularShopSlots_RegularShopSlotId",
                table: "PlayerRegularShopSlots",
                column: "RegularShopSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_RegularShopSlots_ItemId",
                table: "RegularShopSlots",
                column: "ItemId",
                unique: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FarmStates");

            migrationBuilder.DropTable(
                name: "InventorySlots");

            migrationBuilder.DropTable(
                name: "OnlineShopSlots");

            migrationBuilder.DropTable(
                name: "PlayerRegularShopSlots");

            migrationBuilder.DropTable(
                name: "PlayerStates");

            migrationBuilder.DropTable(
                name: "RegularShopSlots");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}
