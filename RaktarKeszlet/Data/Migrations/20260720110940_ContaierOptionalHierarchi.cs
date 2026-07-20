using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaktarKeszlet.Data.Migrations
{
    /// <inheritdoc />
    public partial class ContaierOptionalHierarchi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StorageContainers_Shelves_ShelfId",
                table: "StorageContainers");

            migrationBuilder.AlterColumn<int>(
                name: "ShelfId",
                table: "StorageContainers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "StorageContainers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StorageContainers_CompanyId",
                table: "StorageContainers",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_StorageContainers_Companies_CompanyId",
                table: "StorageContainers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StorageContainers_Shelves_ShelfId",
                table: "StorageContainers",
                column: "ShelfId",
                principalTable: "Shelves",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StorageContainers_Companies_CompanyId",
                table: "StorageContainers");

            migrationBuilder.DropForeignKey(
                name: "FK_StorageContainers_Shelves_ShelfId",
                table: "StorageContainers");

            migrationBuilder.DropIndex(
                name: "IX_StorageContainers_CompanyId",
                table: "StorageContainers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "StorageContainers");

            migrationBuilder.AlterColumn<int>(
                name: "ShelfId",
                table: "StorageContainers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StorageContainers_Shelves_ShelfId",
                table: "StorageContainers",
                column: "ShelfId",
                principalTable: "Shelves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
