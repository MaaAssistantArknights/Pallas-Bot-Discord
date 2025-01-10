using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PallasBot.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddGitHubUserBinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "github_user_binding",
                columns: table => new
                {
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    github_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    github_login = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_github_user_binding", x => new { x.discord_user_id, x.github_user_id });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "github_user_binding");
        }
    }
}
