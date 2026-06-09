using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OctagramDelivery.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRondaStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RondaStocks",
                columns: table => new
                {
                    RondaId = table.Column<int>(type: "int", nullable: false),
                    SeccionId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RondaStocks", x => new { x.RondaId, x.SeccionId, x.ProductoId });
                    table.ForeignKey(
                        name: "FK_RondaStocks_DeliveryRounds_RondaId",
                        column: x => x.RondaId,
                        principalTable: "DeliveryRounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RondaStocks_Products_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RondaStocks_Secciones_SeccionId",
                        column: x => x.SeccionId,
                        principalTable: "Secciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RondaStocks_ProductoId",
                table: "RondaStocks",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_RondaStocks_SeccionId",
                table: "RondaStocks",
                column: "SeccionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RondaStocks");
        }
    }
}
