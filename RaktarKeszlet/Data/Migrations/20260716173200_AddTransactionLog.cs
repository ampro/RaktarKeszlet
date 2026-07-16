using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaktarKeszlet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransactionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    FromStorageContainerId = table.Column<int>(type: "int", nullable: true),
                    ToStorageContainerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionLogs_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionLogs_StorageContainers_FromStorageContainerId",
                        column: x => x.FromStorageContainerId,
                        principalTable: "StorageContainers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransactionLogs_StorageContainers_ToStorageContainerId",
                        column: x => x.ToStorageContainerId,
                        principalTable: "StorageContainers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLogs_FromStorageContainerId",
                table: "TransactionLogs",
                column: "FromStorageContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLogs_ProductId",
                table: "TransactionLogs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLogs_ToStorageContainerId",
                table: "TransactionLogs",
                column: "ToStorageContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLogs_UserId",
                table: "TransactionLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionLogs");
        }
    }
}
