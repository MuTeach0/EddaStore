using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EddaStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderAddressStateOrGovernorate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StateOrGovernorate",
                table: "OrderAddresses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StateOrGovernorate",
                table: "OrderAddresses");
        }
    }
}
