using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OctagramDelivery.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGruposAndSecciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GrupoProductoId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeccionId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GruposProducto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposProducto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GruposProducto_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Secciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Secciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Secciones_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeccionStocks",
                columns: table => new
                {
                    SeccionId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    CantidadStock = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeccionStocks", x => new { x.SeccionId, x.ProductoId });
                    table.ForeignKey(
                        name: "FK_SeccionStocks_Products_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeccionStocks_Secciones_SeccionId",
                        column: x => x.SeccionId,
                        principalTable: "Secciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_GrupoProductoId",
                table: "Products",
                column: "GrupoProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SeccionId",
                table: "Customers",
                column: "SeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposProducto_TenantId",
                table: "GruposProducto",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Secciones_TenantId",
                table: "Secciones",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SeccionStocks_ProductoId",
                table: "SeccionStocks",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Secciones_SeccionId",
                table: "Customers",
                column: "SeccionId",
                principalTable: "Secciones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_GruposProducto_GrupoProductoId",
                table: "Products",
                column: "GrupoProductoId",
                principalTable: "GruposProducto",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Secciones_SeccionId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_GruposProducto_GrupoProductoId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "GruposProducto");

            migrationBuilder.DropTable(
                name: "SeccionStocks");

            migrationBuilder.DropTable(
                name: "Secciones");

            migrationBuilder.DropIndex(
                name: "IX_Products_GrupoProductoId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Customers_SeccionId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "GrupoProductoId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SeccionId",
                table: "Customers");
        }
    }
}
