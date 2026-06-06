using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OctagramDelivery.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPermisosToUsuarioNegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'UsuarioNegocios') AND name = N'PermisosFlags')
    ALTER TABLE [UsuarioNegocios] ADD [PermisosFlags] INT NOT NULL DEFAULT 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'UsuarioNegocios') AND name = N'PermisosFlags')
    ALTER TABLE [UsuarioNegocios] DROP COLUMN [PermisosFlags];");
        }
    }
}
