using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voltiq.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetItemType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "BudgetItem",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "BudgetItem");
        }
    }
}
