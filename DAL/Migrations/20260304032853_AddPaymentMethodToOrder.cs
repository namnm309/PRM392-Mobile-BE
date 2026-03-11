using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "tbl_orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "COD");

            // Update existing records to have COD as default payment method
            migrationBuilder.Sql("UPDATE tbl_orders SET payment_method = 'COD' WHERE payment_method IS NULL OR payment_method = '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "tbl_orders");
        }
    }
}
