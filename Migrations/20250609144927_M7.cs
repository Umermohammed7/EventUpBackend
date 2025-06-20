using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendEventUp.Migrations
{
    /// <inheritdoc />
    public partial class M7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "id_alerte",
                table: "Alerter");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_alerte",
                table: "Alerter",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
