using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EddaStore.Data.Migrations;

public partial class AddAccountProfileFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FirstName",
            table: "AspNetUsers",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "LastName",
            table: "AspNetUsers",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "AspNetUsers",
            type: "bit",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AlterColumn<string>(
            name: "PhoneNumber",
            table: "AspNetUsers",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUsers_PhoneNumber",
            table: "AspNetUsers",
            column: "PhoneNumber",
            unique: true,
            filter: "[PhoneNumber] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "FirstName", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "LastName", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "IsActive", table: "AspNetUsers");
        migrationBuilder.DropIndex(name: "IX_AspNetUsers_PhoneNumber", table: "AspNetUsers");
        migrationBuilder.AlterColumn<string>(
            name: "PhoneNumber",
            table: "AspNetUsers",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);
    }
}
