using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryBrand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_category_brands",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_category_brands", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_category_brands_tbl_brands_brand_id",
                        column: x => x.brand_id,
                        principalTable: "tbl_brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_category_brands_tbl_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "tbl_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_category_brands_brand_id",
                table: "tbl_category_brands",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_category_brands_category_id",
                table: "tbl_category_brands",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_category_brands_category_id_brand_id",
                table: "tbl_category_brands",
                columns: new[] { "category_id", "brand_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_category_brands");
        }
    }
}
