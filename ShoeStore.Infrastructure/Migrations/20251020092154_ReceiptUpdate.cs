using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoeStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReceiptUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "ReceiptDetails",
                newName: "QuantityOrdered");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Receipts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ReceivedQuantity",
                table: "ReceiptDetails",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "ReceivedQuantity",
                table: "ReceiptDetails");

            migrationBuilder.RenameColumn(
                name: "QuantityOrdered",
                table: "ReceiptDetails",
                newName: "Quantity");
        }
    }
}
