using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryParentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "parent_id",
                table: "tbl_categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_categories_parent_id",
                table: "tbl_categories",
                column: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_categories_tbl_categories_parent_id",
                table: "tbl_categories",
                column: "parent_id",
                principalTable: "tbl_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_categories_tbl_categories_parent_id",
                table: "tbl_categories");

            migrationBuilder.DropIndex(
                name: "IX_tbl_categories_parent_id",
                table: "tbl_categories");

            migrationBuilder.DropColumn(
                name: "parent_id",
                table: "tbl_categories");
        }
    }
}
