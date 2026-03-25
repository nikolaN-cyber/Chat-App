using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelationToUserStatusInUserTableAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__userStatuses_UserId",
                table: "_userStatuses");

            migrationBuilder.CreateIndex(
                name: "IX__userStatuses_UserId",
                table: "_userStatuses",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__userStatuses_UserId",
                table: "_userStatuses");

            migrationBuilder.CreateIndex(
                name: "IX__userStatuses_UserId",
                table: "_userStatuses",
                column: "UserId");
        }
    }
}
