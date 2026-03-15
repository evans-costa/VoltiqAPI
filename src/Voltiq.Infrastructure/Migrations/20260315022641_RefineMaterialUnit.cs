using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voltiq.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefineMaterialUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""Materials"" ALTER COLUMN ""Unit"" TYPE integer USING 1;");

            migrationBuilder.Sql(@"ALTER TABLE ""BudgetItem"" ALTER COLUMN ""Unit"" TYPE integer USING NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "Materials",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "BudgetItem",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
