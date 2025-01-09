using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PallasBot.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dynamic_configuration",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dynamic_configuration", x => new { x.guild_id, x.key });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dynamic_configuration");
        }
    }
}
