using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SvadbeniSalon.Services.Migrations
{
    /// <inheritdoc />
    public partial class HashPasswordResetCodesProper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_UserId_Code",
                table: "PasswordResetTokens");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PasswordResetTokens");

            migrationBuilder.AddColumn<string>(
                name: "CodeHash",
                table: "PasswordResetTokens",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodeSalt",
                table: "PasswordResetTokens",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId_ExpiresAt",
                table: "PasswordResetTokens",
                columns: new[] { "UserId", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_UserId_ExpiresAt",
                table: "PasswordResetTokens");

            migrationBuilder.DropColumn(
                name: "CodeHash",
                table: "PasswordResetTokens");

            migrationBuilder.DropColumn(
                name: "CodeSalt",
                table: "PasswordResetTokens");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PasswordResetTokens",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId_Code",
                table: "PasswordResetTokens",
                columns: new[] { "UserId", "Code" });
        }
    }
}
