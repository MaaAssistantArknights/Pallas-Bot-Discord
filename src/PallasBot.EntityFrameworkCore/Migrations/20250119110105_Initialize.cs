using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PallasBot.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Initialize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "discord_user_role",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    role_ids = table.Column<decimal[]>(type: "numeric(20,0)[]", nullable: false),
                    update_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discord_user_role", x => new { x.guild_id, x.user_id });
                });

            migrationBuilder.CreateTable(
                name: "dynamic_configuration",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    update_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    update_by = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dynamic_configuration", x => new { x.guild_id, x.key });
                });

            migrationBuilder.CreateTable(
                name: "github_contributors",
                columns: table => new
                {
                    github_login = table.Column<string>(type: "text", nullable: false),
                    is_organization_member = table.Column<bool>(type: "boolean", nullable: false),
                    contribute_to = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_github_contributors", x => x.github_login);
                });

            migrationBuilder.CreateTable(
                name: "github_user_binding",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    discord_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    github_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    github_login = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_github_user_binding", x => new { x.guild_id, x.discord_user_id });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discord_user_role");

            migrationBuilder.DropTable(
                name: "dynamic_configuration");

            migrationBuilder.DropTable(
                name: "github_contributors");

            migrationBuilder.DropTable(
                name: "github_user_binding");
        }
    }
}
