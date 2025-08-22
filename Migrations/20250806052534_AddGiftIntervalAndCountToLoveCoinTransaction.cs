using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KlondaikLyubvi.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftIntervalAndCountToLoveCoinTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    LovePoints = table.Column<int>(type: "INTEGER", nullable: false),
                    LastVisit = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BucketItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Progress = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BucketItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BucketItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoveNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoveNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoveNotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Price = table.Column<int>(type: "INTEGER", nullable: false),
                    Emoji = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoveCoinTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    StoreItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsGift = table.Column<bool>(type: "INTEGER", nullable: false),
                    ToUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExecutionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsExecuted = table.Column<bool>(type: "INTEGER", nullable: false),
                    GiftStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GiftEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GiftCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoveCoinTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoveCoinTransactions_StoreItems_StoreItemId",
                        column: x => x.StoreItemId,
                        principalTable: "StoreItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoveCoinTransactions_Users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LoveCoinTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "DisplayName", "LastVisit", "LovePoints", "PasswordHash", "UserName" },
                values: new object[,]
                {
                    { 1, "Денис", null, 12, "denis", "denis" },
                    { 2, "Лиза", null, 12, "liza", "liza" }
                });

            migrationBuilder.InsertData(
                table: "StoreItems",
                columns: new[] { "Id", "Description", "Emoji", "Name", "Price", "UserId" },
                values: new object[,]
                {
                    { 1, "Расслабляющий массаж от вашего любимого человека", "💆‍♀️", "💆‍♀️ Массаж на 20 минут", 5, 1 },
                    { 2, "Вкусный завтрак и кофе, приготовленные с любовью", "🍳", "🍳 Завтрак в постель", 4, 1 },
                    { 3, "Выбор фильма, плед и объятия", "🎥", "🎥 Вечер фильмов", 3, 2 },
                    { 4, "Свечи, музыка и расслабление вдвоём", "🛁", "🛁 Совместная ванна", 7, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BucketItems_UserId",
                table: "BucketItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_UserId",
                table: "Events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoveCoinTransactions_StoreItemId",
                table: "LoveCoinTransactions",
                column: "StoreItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LoveCoinTransactions_ToUserId",
                table: "LoveCoinTransactions",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoveCoinTransactions_UserId",
                table: "LoveCoinTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoveNotes_UserId",
                table: "LoveNotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_UserId",
                table: "Photos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreItems_UserId",
                table: "StoreItems",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BucketItems");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "LoveCoinTransactions");

            migrationBuilder.DropTable(
                name: "LoveNotes");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "StoreItems");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
