using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOM2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class hostLegacySSH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ManagedHosts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "LegacySshSupported",
                table: "ManagedHosts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegacySshSupported",
                table: "ManagedHosts");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ManagedHosts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
