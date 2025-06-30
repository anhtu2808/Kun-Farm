using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KunFarm.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFarmState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 6, 30, 0, 21, 3, 860, DateTimeKind.Local).AddTicks(4680),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 6, 29, 23, 40, 6, 676, DateTimeKind.Local).AddTicks(3120));

            migrationBuilder.CreateTable(
                name: "FarmStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TileStatesJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlantsJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastSaved = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValue: new DateTime(2025, 6, 29, 17, 21, 3, 860, DateTimeKind.Utc).AddTicks(5540))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmStates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FarmStates_UserId",
                table: "FarmStates",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FarmStates");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastSaved",
                table: "PlayerStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2025, 6, 29, 23, 40, 6, 676, DateTimeKind.Local).AddTicks(3120),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2025, 6, 30, 0, 21, 3, 860, DateTimeKind.Local).AddTicks(4680));
        }
    }
}
