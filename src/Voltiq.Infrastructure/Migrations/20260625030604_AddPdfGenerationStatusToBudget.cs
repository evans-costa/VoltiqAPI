using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voltiq.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfGenerationStatusToBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PdfGenerationStatus",
                table: "Budgets",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfGenerationStatus",
                table: "Budgets");
        }
    }
}
