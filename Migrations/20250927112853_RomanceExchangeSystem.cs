using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KlondaikLyubvi.Migrations
{
    /// <inheritdoc />
    public partial class RomanceExchangeSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoveCoinTransactions");

            migrationBuilder.DropTable(
                name: "StoreItems");

            migrationBuilder.RenameColumn(
                name: "LovePoints",
                table: "Users",
                newName: "TrustCredits");

            migrationBuilder.CreateTable(
                name: "ServiceOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    TrustCost = table.Column<int>(type: "INTEGER", nullable: false),
                    Emoji = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceOffers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceExchanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequesterId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProviderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceOfferId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestMessage = table.Column<string>(type: "TEXT", nullable: true),
                    ResponseMessage = table.Column<string>(type: "TEXT", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Rating = table.Column<int>(type: "INTEGER", nullable: true),
                    Review = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceExchanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceExchanges_ServiceOffers_ServiceOfferId",
                        column: x => x.ServiceOfferId,
                        principalTable: "ServiceOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceExchanges_Users_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceExchanges_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ServiceOffers",
                columns: new[] { "Id", "Category", "Description", "Emoji", "IsActive", "Name", "TrustCost", "UserId" },
                values: new object[,]
                {
                    { 1, "Релакс", "Расслабляющий массаж спины и плеч", "💆‍♀️", true, "Массаж на 20 минут", 1, 1 },
                    { 2, "Забота", "Вкусный завтрак и кофе, приготовленные с любовью", "🍳", true, "Завтрак в постель", 1, 1 },
                    { 3, "Досуг", "Выбор фильма, плед и объятия", "🎥", true, "Вечер фильмов", 1, 2 },
                    { 4, "Романтика", "Свечи, музыка и расслабление вдвоём", "🛁", true, "Совместная ванна", 2, 2 },
                    { 5, "Забота", "Приготовлю твое любимое блюдо", "🍽️", true, "Домашний ужин", 1, 2 },
                    { 6, "Романтика", "Романтическая прогулка в красивом месте", "🌟", true, "Прогулка под звездами", 1, 1 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "TrustCredits",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "TrustCredits",
                value: 5);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExchanges_ProviderId",
                table: "ServiceExchanges",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExchanges_RequesterId",
                table: "ServiceExchanges",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExchanges_ServiceOfferId",
                table: "ServiceExchanges",
                column: "ServiceOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOffers_UserId",
                table: "ServiceOffers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceExchanges");

            migrationBuilder.DropTable(
                name: "ServiceOffers");

            migrationBuilder.RenameColumn(
                name: "TrustCredits",
                table: "Users",
                newName: "LovePoints");

            migrationBuilder.CreateTable(
                name: "StoreItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Emoji = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<int>(type: "INTEGER", nullable: false)
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
                    StoreItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExecutionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GiftCount = table.Column<int>(type: "INTEGER", nullable: false),
                    GiftEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GiftStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsExecuted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsGift = table.Column<bool>(type: "INTEGER", nullable: false)
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
                table: "StoreItems",
                columns: new[] { "Id", "Description", "Emoji", "Name", "Price", "UserId" },
                values: new object[,]
                {
                    { 1, "Расслабляющий массаж от вашего любимого человека", "💆‍♀️", "💆‍♀️ Массаж на 20 минут", 5, 1 },
                    { 2, "Вкусный завтрак и кофе, приготовленные с любовью", "🍳", "🍳 Завтрак в постель", 4, 1 },
                    { 3, "Выбор фильма, плед и объятия", "🎥", "🎥 Вечер фильмов", 3, 2 },
                    { 4, "Свечи, музыка и расслабление вдвоём", "🛁", "🛁 Совместная ванна", 7, 2 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "LovePoints",
                value: 12);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "LovePoints",
                value: 12);

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
                name: "IX_StoreItems_UserId",
                table: "StoreItems",
                column: "UserId");
        }
    }
}
