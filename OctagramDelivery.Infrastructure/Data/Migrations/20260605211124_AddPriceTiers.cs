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
            // Eliminar columna PrecioEspecial solo si existe (maneja DEFAULT constraint automáticamente)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'CustomerProducts') AND name = N'PrecioEspecial')
BEGIN
    DECLARE @con NVARCHAR(256);
    SELECT @con = d.name
    FROM sys.default_constraints d
    INNER JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
    WHERE d.parent_object_id = OBJECT_ID(N'CustomerProducts') AND c.name = N'PrecioEspecial';
    IF @con IS NOT NULL EXEC(N'ALTER TABLE [CustomerProducts] DROP CONSTRAINT [' + @con + N']');
    ALTER TABLE [CustomerProducts] DROP COLUMN [PrecioEspecial];
END");

            // Agregar columna PriceTierId solo si no existe
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'CustomerProducts') AND name = N'PriceTierId')
    ALTER TABLE [CustomerProducts] ADD [PriceTierId] INT NULL;");

            // Crear tabla PriceTiers solo si no existe
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'PriceTiers') AND type = N'U')
CREATE TABLE [PriceTiers] (
    [Id]        INT            NOT NULL IDENTITY(1,1),
    [ProductId] INT            NOT NULL,
    [Numero]    INT            NOT NULL,
    [Etiqueta]  NVARCHAR(MAX)  NULL,
    [Precio]    DECIMAL(18,2)  NOT NULL,
    CONSTRAINT [PK_PriceTiers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PriceTiers_Products_ProductId] FOREIGN KEY ([ProductId])
        REFERENCES [Products]([Id]) ON DELETE CASCADE
);");

            // Índice en CustomerProducts.PriceTierId
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerProducts_PriceTierId' AND object_id = OBJECT_ID(N'CustomerProducts'))
    CREATE INDEX [IX_CustomerProducts_PriceTierId] ON [CustomerProducts]([PriceTierId]);");

            // Índice único en PriceTiers(ProductId, Numero)
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PriceTiers_ProductId_Numero' AND object_id = OBJECT_ID(N'PriceTiers'))
    CREATE UNIQUE INDEX [IX_PriceTiers_ProductId_Numero] ON [PriceTiers]([ProductId], [Numero]);");

            // FK de CustomerProducts → PriceTiers con ON DELETE NO ACTION
            // (SET NULL no es válido en SQL Server por rutas de cascada múltiples:
            //  Products → PriceTiers CASCADE y Customers → CustomerProducts CASCADE)
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CustomerProducts_PriceTiers_PriceTierId')
    ALTER TABLE [CustomerProducts] ADD CONSTRAINT [FK_CustomerProducts_PriceTiers_PriceTierId]
        FOREIGN KEY ([PriceTierId]) REFERENCES [PriceTiers]([Id]) ON DELETE NO ACTION;");
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
