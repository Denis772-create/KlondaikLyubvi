using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KlondaikLyubvi.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftIntervalAndCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Occasion = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MetaTitle = table.Column<string>(type: "TEXT", nullable: true),
                    MetaDescription = table.Column<string>(type: "TEXT", nullable: true),
                    MetaImage = table.Column<string>(type: "TEXT", nullable: true),
                    MetaSiteName = table.Column<string>(type: "TEXT", nullable: true),
                    MetaUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "StoreItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "💆‍♀️ Массаж на 20 минут");

            migrationBuilder.UpdateData(
                table: "StoreItems",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "🍳 Завтрак в постель");

            migrationBuilder.UpdateData(
                table: "StoreItems",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "🎥 Вечер фильмов");

            migrationBuilder.UpdateData(
                table: "StoreItems",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "🛁 Совместная ванна");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserId",
                table: "WishlistItems",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.UpdateData(
                table: "StoreItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Массаж на 20 минут");

            migrationBuilder.UpdateData(
                table: "StoreItems",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Завтрак в постель");

            migrationBuilder.UpdateData(
                table: "StoreItems",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Вечер фильмов");

            migrationBuilder.UpdateData(
                table: "StoreItems",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Совместная ванна");
        }
    }
}
