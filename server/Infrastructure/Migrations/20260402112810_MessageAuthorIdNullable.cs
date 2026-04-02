using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MessageAuthorIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__messages__users_AuthorId",
                table: "_messages");

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "_messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK__messages__users_AuthorId",
                table: "_messages",
                column: "AuthorId",
                principalTable: "_users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__messages__users_AuthorId",
                table: "_messages");

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "_messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK__messages__users_AuthorId",
                table: "_messages",
                column: "AuthorId",
                principalTable: "_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
