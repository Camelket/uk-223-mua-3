using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L_Bank_W_Backend.DbAccess.Migrations
{
    /// <inheritdoc />
    public partial class SomeMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Ledgers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Ledgers_UserId",
                table: "Ledgers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ledgers_Users_UserId",
                table: "Ledgers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ledgers_Users_UserId",
                table: "Ledgers");

            migrationBuilder.DropIndex(
                name: "IX_Ledgers_UserId",
                table: "Ledgers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Ledgers");
        }
    }
}
