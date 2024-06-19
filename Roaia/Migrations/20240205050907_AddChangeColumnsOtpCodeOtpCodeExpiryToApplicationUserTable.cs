using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roaia.Migrations
{
    /// <inheritdoc />
    public partial class AddChangeColumnsOtpCodeOtpCodeExpiryToApplicationUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResetPasswordTokenExpiry",
                table: "AspNetUsers",
                newName: "OtpCodeExpiry");

            migrationBuilder.RenameColumn(
                name: "ResetPasswordToken",
                table: "AspNetUsers",
                newName: "OtpCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OtpCodeExpiry",
                table: "AspNetUsers",
                newName: "ResetPasswordTokenExpiry");

            migrationBuilder.RenameColumn(
                name: "OtpCode",
                table: "AspNetUsers",
                newName: "ResetPasswordToken");
        }
    }
}
