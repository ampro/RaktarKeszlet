using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaktarKeszlet.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProductsHierarchiMod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuildingId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShelfId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StorageContainerId1",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_BuildingId",
                table: "Products",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CompanyId",
                table: "Products",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_RoomId",
                table: "Products",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ShelfId",
                table: "Products",
                column: "ShelfId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_StorageContainerId1",
                table: "Products",
                column: "StorageContainerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Buildings_BuildingId",
                table: "Products",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Companies_CompanyId",
                table: "Products",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Rooms_RoomId",
                table: "Products",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Shelves_ShelfId",
                table: "Products",
                column: "ShelfId",
                principalTable: "Shelves",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_StorageContainers_StorageContainerId1",
                table: "Products",
                column: "StorageContainerId1",
                principalTable: "StorageContainers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Buildings_BuildingId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Companies_CompanyId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Rooms_RoomId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Shelves_ShelfId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_StorageContainers_StorageContainerId1",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_BuildingId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CompanyId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_RoomId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ShelfId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_StorageContainerId1",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShelfId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StorageContainerId1",
                table: "Products");
        }
    }
}
