using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OctagramDelivery.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNegocioPermiso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NegocioPermisos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false),
                    PermisosFlags = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NegocioPermisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NegocioPermisos_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NegocioPermisos_TenantId_Rol",
                table: "NegocioPermisos",
                columns: new[] { "TenantId", "Rol" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NegocioPermisos");
        }
    }
}
