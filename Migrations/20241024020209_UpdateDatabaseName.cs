using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMManagementApplication.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransferTo",
                table: "Transactions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransferTo",
                table: "Transactions");
        }
    }
}
