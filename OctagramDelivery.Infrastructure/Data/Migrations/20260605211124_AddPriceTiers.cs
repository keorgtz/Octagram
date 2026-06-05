using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OctagramDelivery.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioEspecial",
                table: "CustomerProducts");

            migrationBuilder.AddColumn<int>(
                name: "PriceTierId",
                table: "CustomerProducts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PriceTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Etiqueta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceTiers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProducts_PriceTierId",
                table: "CustomerProducts",
                column: "PriceTierId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceTiers_ProductId_Numero",
                table: "PriceTiers",
                columns: new[] { "ProductId", "Numero" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerProducts_PriceTiers_PriceTierId",
                table: "CustomerProducts",
                column: "PriceTierId",
                principalTable: "PriceTiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerProducts_PriceTiers_PriceTierId",
                table: "CustomerProducts");

            migrationBuilder.DropTable(
                name: "PriceTiers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProducts_PriceTierId",
                table: "CustomerProducts");

            migrationBuilder.DropColumn(
                name: "PriceTierId",
                table: "CustomerProducts");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioEspecial",
                table: "CustomerProducts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }
    }
}
