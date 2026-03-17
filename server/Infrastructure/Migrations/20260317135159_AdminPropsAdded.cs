using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdminPropsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__messages__conversation_ConversationId",
                table: "_messages");

            migrationBuilder.AlterColumn<int>(
                name: "ConversationId",
                table: "_messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "_conversation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX__conversation_AdminId",
                table: "_conversation",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK__conversation__users_AdminId",
                table: "_conversation",
                column: "AdminId",
                principalTable: "_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK__messages__conversation_ConversationId",
                table: "_messages",
                column: "ConversationId",
                principalTable: "_conversation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__conversation__users_AdminId",
                table: "_conversation");

            migrationBuilder.DropForeignKey(
                name: "FK__messages__conversation_ConversationId",
                table: "_messages");

            migrationBuilder.DropIndex(
                name: "IX__conversation_AdminId",
                table: "_conversation");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "_conversation");

            migrationBuilder.AlterColumn<int>(
                name: "ConversationId",
                table: "_messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK__messages__conversation_ConversationId",
                table: "_messages",
                column: "ConversationId",
                principalTable: "_conversation",
                principalColumn: "Id");
        }
    }
}
