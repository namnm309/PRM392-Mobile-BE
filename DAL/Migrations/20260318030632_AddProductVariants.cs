using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tbl_cart_items_user_id_product_id",
                table: "tbl_cart_items");

            migrationBuilder.AddColumn<Guid>(
                name: "variant_id",
                table: "tbl_order_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "variant_id",
                table: "tbl_cart_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_product_variants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    variant_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    color_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    color_hex = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ram_gb = table.Column<int>(type: "integer", nullable: true),
                    storage_gb = table.Column<int>(type: "integer", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    stock = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_product_variants", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_product_variants_tbl_products_product_id",
                        column: x => x.product_id,
                        principalTable: "tbl_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_order_items_variant_id",
                table: "tbl_order_items",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_cart_items_user_id_product_id_variant_id",
                table: "tbl_cart_items",
                columns: new[] { "user_id", "product_id", "variant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_cart_items_variant_id",
                table: "tbl_cart_items",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_variants_product_id",
                table: "tbl_product_variants",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_variants_product_id_color_name_ram_gb_storage_gb",
                table: "tbl_product_variants",
                columns: new[] { "product_id", "color_name", "ram_gb", "storage_gb" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_variants_product_id_display_order",
                table: "tbl_product_variants",
                columns: new[] { "product_id", "display_order" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_variants_product_id_is_active",
                table: "tbl_product_variants",
                columns: new[] { "product_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_variants_product_id_sku",
                table: "tbl_product_variants",
                columns: new[] { "product_id", "sku" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_cart_items_tbl_product_variants_variant_id",
                table: "tbl_cart_items",
                column: "variant_id",
                principalTable: "tbl_product_variants",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_order_items_tbl_product_variants_variant_id",
                table: "tbl_order_items",
                column: "variant_id",
                principalTable: "tbl_product_variants",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_cart_items_tbl_product_variants_variant_id",
                table: "tbl_cart_items");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_order_items_tbl_product_variants_variant_id",
                table: "tbl_order_items");

            migrationBuilder.DropTable(
                name: "tbl_product_variants");

            migrationBuilder.DropIndex(
                name: "IX_tbl_order_items_variant_id",
                table: "tbl_order_items");

            migrationBuilder.DropIndex(
                name: "IX_tbl_cart_items_user_id_product_id_variant_id",
                table: "tbl_cart_items");

            migrationBuilder.DropIndex(
                name: "IX_tbl_cart_items_variant_id",
                table: "tbl_cart_items");

            migrationBuilder.DropColumn(
                name: "variant_id",
                table: "tbl_order_items");

            migrationBuilder.DropColumn(
                name: "variant_id",
                table: "tbl_cart_items");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_cart_items_user_id_product_id",
                table: "tbl_cart_items",
                columns: new[] { "user_id", "product_id" });
        }
    }
}
