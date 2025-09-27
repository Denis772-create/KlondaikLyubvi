using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KlondaikLyubvi.Migrations
{
    /// <inheritdoc />
    public partial class BarterSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceExchanges_ServiceOffers_ServiceOfferId",
                table: "ServiceExchanges");

            migrationBuilder.DropColumn(
                name: "TrustCredits",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TrustCost",
                table: "ServiceOffers");

            migrationBuilder.RenameColumn(
                name: "ServiceOfferId",
                table: "ServiceExchanges",
                newName: "RequestedServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceExchanges_ServiceOfferId",
                table: "ServiceExchanges",
                newName: "IX_ServiceExchanges_RequestedServiceId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ServiceOffers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "OfferedServiceId",
                table: "ServiceExchanges",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ServiceOffers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 27, 11, 49, 41, 76, DateTimeKind.Utc).AddTicks(4639));

            migrationBuilder.UpdateData(
                table: "ServiceOffers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 27, 11, 49, 41, 76, DateTimeKind.Utc).AddTicks(4642));

            migrationBuilder.UpdateData(
                table: "ServiceOffers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 27, 11, 49, 41, 76, DateTimeKind.Utc).AddTicks(4644));

            migrationBuilder.UpdateData(
                table: "ServiceOffers",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 27, 11, 49, 41, 76, DateTimeKind.Utc).AddTicks(4646));

            migrationBuilder.UpdateData(
                table: "ServiceOffers",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 27, 11, 49, 41, 76, DateTimeKind.Utc).AddTicks(4647));

            migrationBuilder.UpdateData(
                table: "ServiceOffers",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 27, 11, 49, 41, 76, DateTimeKind.Utc).AddTicks(4649));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExchanges_OfferedServiceId",
                table: "ServiceExchanges",
                column: "OfferedServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceExchanges_ServiceOffers_OfferedServiceId",
                table: "ServiceExchanges",
                column: "OfferedServiceId",
                principalTable: "ServiceOffers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceExchanges_ServiceOffers_RequestedServiceId",
                table: "ServiceExchanges",
                column: "RequestedServiceId",
                principalTable: "ServiceOffers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceExchanges_ServiceOffers_OfferedServiceId",
                table: "ServiceExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceExchanges_ServiceOffers_RequestedServiceId",
                table: "ServiceExchanges");

            migrationBuilder.DropIndex(
                name: "IX_ServiceExchanges_OfferedServiceId",
                table: "ServiceExchanges");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ServiceOffers");

            migrationBuilder.DropColumn(
                name: "OfferedServiceId",
                table: "ServiceExchanges");

            migrationBuilder.RenameColumn(
                name: "RequestedServiceId",
                table: "ServiceExchanges",
                newName: "ServiceOfferId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceExchanges_RequestedServiceId",
                table: "ServiceExchanges",
                newName: "IX_ServiceExchanges_ServiceOfferId");

            migrationBuilder.AddColumn<int>(
                name: "TrustCredits",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrustCost",
                table: "ServiceOffers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ServiceOffers",
                keyColumn: "Id",
                keyValue: 1,
                column: "TrustCost",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ServiceOffers",
                keyColumn: "Id",
                keyValue: 2,
                column: "TrustCost",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ServiceOffers",
                keyColumn: "Id",
                keyValue: 3,
                column: "TrustCost",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ServiceOffers",
                keyColumn: "Id",
                keyValue: 4,
                column: "TrustCost",
                value: 2);

            migrationBuilder.UpdateData(
                table: "ServiceOffers",
                keyColumn: "Id",
                keyValue: 5,
                column: "TrustCost",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ServiceOffers",
                keyColumn: "Id",
                keyValue: 6,
                column: "TrustCost",
                value: 1);

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

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceExchanges_ServiceOffers_ServiceOfferId",
                table: "ServiceExchanges",
                column: "ServiceOfferId",
                principalTable: "ServiceOffers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
