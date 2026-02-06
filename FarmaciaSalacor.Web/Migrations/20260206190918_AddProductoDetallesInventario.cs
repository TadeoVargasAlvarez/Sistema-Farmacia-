using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmaciaSalacor.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddProductoDetallesInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Concentracion",
                table: "Productos",
                type: "TEXT",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormaFarmaceutica",
                table: "Productos",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreGenerico",
                table: "Productos",
                type: "TEXT",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Presentacion",
                table: "Productos",
                type: "TEXT",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Concentracion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FormaFarmaceutica",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "NombreGenerico",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Presentacion",
                table: "Productos");
        }
    }
}
