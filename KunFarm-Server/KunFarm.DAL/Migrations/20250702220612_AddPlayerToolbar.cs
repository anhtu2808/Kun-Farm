using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KunFarm.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerToolbar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 3, 5, 6, 11, 718, DateTimeKind.Local).AddTicks(4810),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 3, 4, 33, 31, 126, DateTimeKind.Local).AddTicks(740));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "FarmStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 2, 22, 6, 11, 720, DateTimeKind.Utc).AddTicks(430),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 2, 21, 33, 31, 126, DateTimeKind.Utc).AddTicks(2990));

            migrationBuilder.CreateTable(
                name: "PlayerToolbars",
                columns: table => new
                {
                    PlayerStateId = table.Column<int>(type: "int", nullable: false),
                    ToolsJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastSaved = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValue: new DateTime(2025, 7, 3, 5, 6, 11, 719, DateTimeKind.Local).AddTicks(340))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerToolbars", x => x.PlayerStateId);
                    table.ForeignKey(
                        name: "FK_PlayerToolbars_PlayerStates_PlayerStateId",
                        column: x => x.PlayerStateId,
                        principalTable: "PlayerStates",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerToolbars");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 3, 4, 33, 31, 126, DateTimeKind.Local).AddTicks(740),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 3, 5, 6, 11, 718, DateTimeKind.Local).AddTicks(4810));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "FarmStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 7, 2, 21, 33, 31, 126, DateTimeKind.Utc).AddTicks(2990),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 7, 2, 22, 6, 11, 720, DateTimeKind.Utc).AddTicks(430));
        }
    }
}
