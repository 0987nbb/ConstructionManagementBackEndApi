using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteProgressPercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProgressPercentage",
                table: "Sites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sites_ProgressPercentage",
                table: "Sites",
                sql: "[ProgressPercentage] >= 0 AND [ProgressPercentage] <= 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Sites_ProgressPercentage",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "ProgressPercentage",
                table: "Sites");
        }
    }
}
