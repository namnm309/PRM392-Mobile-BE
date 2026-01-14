using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class SomeEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_addresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    address_line1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    address_line2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_addresses", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_addresses_tbl_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    stock = table.Column<int>(type: "integer", nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_on_sale = table.Column<bool>(type: "boolean", nullable: false),
                    no_voucher_tag = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_vouchers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    discount_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    min_order_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_usage_limit = table.Column<int>(type: "integer", nullable: false),
                    per_user_limit = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_vouchers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_cart_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price_snapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_cart_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_cart_items_tbl_products_product_id",
                        column: x => x.product_id,
                        principalTable: "tbl_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_cart_items_tbl_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_comments_tbl_products_product_id",
                        column: x => x.product_id,
                        principalTable: "tbl_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_comments_tbl_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    voucher_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_orders_tbl_addresses_address_id",
                        column: x => x.address_id,
                        principalTable: "tbl_addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_orders_tbl_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_orders_tbl_vouchers_voucher_id",
                        column: x => x.voucher_id,
                        principalTable: "tbl_vouchers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_comment_replies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reply_content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_comment_replies", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_comment_replies_tbl_comments_comment_id",
                        column: x => x.comment_id,
                        principalTable: "tbl_comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_comment_replies_tbl_users_staff_id",
                        column: x => x.staff_id,
                        principalTable: "tbl_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_order_items_tbl_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "tbl_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_order_items_tbl_products_product_id",
                        column: x => x.product_id,
                        principalTable: "tbl_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_voucher_usages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voucher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_voucher_usages", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_voucher_usages_tbl_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "tbl_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_voucher_usages_tbl_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_voucher_usages_tbl_vouchers_voucher_id",
                        column: x => x.voucher_id,
                        principalTable: "tbl_vouchers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_addresses_user_id",
                table: "tbl_addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_cart_items_product_id",
                table: "tbl_cart_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_cart_items_user_id_product_id",
                table: "tbl_cart_items",
                columns: new[] { "user_id", "product_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_comment_replies_comment_id",
                table: "tbl_comment_replies",
                column: "comment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_comment_replies_staff_id",
                table: "tbl_comment_replies",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_comments_product_id",
                table: "tbl_comments",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_comments_user_id_product_id",
                table: "tbl_comments",
                columns: new[] { "user_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_order_items_order_id",
                table: "tbl_order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_order_items_product_id_status",
                table: "tbl_order_items",
                columns: new[] { "product_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_orders_address_id",
                table: "tbl_orders",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_orders_created_at",
                table: "tbl_orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_orders_user_id",
                table: "tbl_orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_orders_voucher_id",
                table: "tbl_orders",
                column: "voucher_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_voucher_usages_order_id",
                table: "tbl_voucher_usages",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_voucher_usages_user_id",
                table: "tbl_voucher_usages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_voucher_usages_voucher_id_user_id",
                table: "tbl_voucher_usages",
                columns: new[] { "voucher_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_vouchers_code",
                table: "tbl_vouchers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_vouchers_is_active_start_time_end_time",
                table: "tbl_vouchers",
                columns: new[] { "is_active", "start_time", "end_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_cart_items");

            migrationBuilder.DropTable(
                name: "tbl_comment_replies");

            migrationBuilder.DropTable(
                name: "tbl_order_items");

            migrationBuilder.DropTable(
                name: "tbl_voucher_usages");

            migrationBuilder.DropTable(
                name: "tbl_comments");

            migrationBuilder.DropTable(
                name: "tbl_orders");

            migrationBuilder.DropTable(
                name: "tbl_products");

            migrationBuilder.DropTable(
                name: "tbl_addresses");

            migrationBuilder.DropTable(
                name: "tbl_vouchers");
        }
    }
}
