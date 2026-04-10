using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roaia.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxContactsAndSubscriptionExpiryDateToGlassesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxContacts",
                table: "Glasses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionExpiryDate",
                table: "Glasses",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxContacts",
                table: "Glasses");

            migrationBuilder.DropColumn(
                name: "SubscriptionExpiryDate",
                table: "Glasses");
        }
    }
}
