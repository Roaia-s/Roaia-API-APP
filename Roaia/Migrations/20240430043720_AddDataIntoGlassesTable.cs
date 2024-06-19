using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roaia.Migrations
{
    /// <inheritdoc />
    public partial class AddDataIntoGlassesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .Sql("INSERT INTO Glasses (Id, FullName, Age, Gender, ImageUrl) VALUES ('4c215575-2575-4762-8f22-ace41c', 'Roaia', 15, 'Male', '/images/avatar.png')");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Glasses WHERE Id = '4c215575-2575-4762-8f22-ace41c'");
        }
    }
}
