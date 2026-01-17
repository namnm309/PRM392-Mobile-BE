using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProductsCategoriesBrandsImagesOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "brand_id",
                table: "tbl_products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancel_reason",
                table: "tbl_orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at",
                table: "tbl_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cancelled_by",
                table: "tbl_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_brands",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_brands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_product_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    image_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_product_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_product_images_tbl_products_product_id",
                        column: x => x.product_id,
                        principalTable: "tbl_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_brand_id",
                table: "tbl_products",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_category_id",
                table: "tbl_products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_is_active_category_id_brand_id",
                table: "tbl_products",
                columns: new[] { "is_active", "category_id", "brand_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_brands_is_active",
                table: "tbl_brands",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_brands_name",
                table: "tbl_brands",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_categories_is_active",
                table: "tbl_categories",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_categories_name",
                table: "tbl_categories",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_images_product_id",
                table: "tbl_product_images",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_images_product_id_display_order",
                table: "tbl_product_images",
                columns: new[] { "product_id", "display_order" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_images_product_id_image_type",
                table: "tbl_product_images",
                columns: new[] { "product_id", "image_type" });

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_products_tbl_brands_brand_id",
                table: "tbl_products",
                column: "brand_id",
                principalTable: "tbl_brands",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_products_tbl_categories_category_id",
                table: "tbl_products",
                column: "category_id",
                principalTable: "tbl_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_products_tbl_brands_brand_id",
                table: "tbl_products");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_products_tbl_categories_category_id",
                table: "tbl_products");

            migrationBuilder.DropTable(
                name: "tbl_brands");

            migrationBuilder.DropTable(
                name: "tbl_categories");

            migrationBuilder.DropTable(
                name: "tbl_product_images");

            migrationBuilder.DropIndex(
                name: "IX_tbl_products_brand_id",
                table: "tbl_products");

            migrationBuilder.DropIndex(
                name: "IX_tbl_products_category_id",
                table: "tbl_products");

            migrationBuilder.DropIndex(
                name: "IX_tbl_products_is_active_category_id_brand_id",
                table: "tbl_products");

            migrationBuilder.DropColumn(
                name: "brand_id",
                table: "tbl_products");

            migrationBuilder.DropColumn(
                name: "cancel_reason",
                table: "tbl_orders");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "tbl_orders");

            migrationBuilder.DropColumn(
                name: "cancelled_by",
                table: "tbl_orders");
        }
    }
}
