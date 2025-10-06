using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE.BOs.Migrations
{
    /// <inheritdoc />
    public partial class AddLicensePlateToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LicensePlate",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicensePlate",
                table: "Products");
        }
    }
}
