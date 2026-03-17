using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameCommentToReviewAndAddProductComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop FK and indexes on tbl_comment_replies first (child table)
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_comment_replies_tbl_comments_comment_id",
                table: "tbl_comment_replies");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_comment_replies_tbl_users_staff_id",
                table: "tbl_comment_replies");

            migrationBuilder.DropIndex(
                name: "IX_tbl_comment_replies_comment_id",
                table: "tbl_comment_replies");

            migrationBuilder.DropIndex(
                name: "IX_tbl_comment_replies_staff_id",
                table: "tbl_comment_replies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_comment_replies",
                table: "tbl_comment_replies");

            // 2. Drop FK and indexes on tbl_comments (parent table)
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_comments_tbl_products_product_id",
                table: "tbl_comments");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_comments_tbl_users_user_id",
                table: "tbl_comments");

            migrationBuilder.DropIndex(
                name: "IX_tbl_comments_product_id",
                table: "tbl_comments");

            migrationBuilder.DropIndex(
                name: "IX_tbl_comments_user_id_product_id",
                table: "tbl_comments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_comments",
                table: "tbl_comments");

            // 3. Rename tables
            migrationBuilder.RenameTable(
                name: "tbl_comments",
                newName: "tbl_reviews");

            migrationBuilder.RenameTable(
                name: "tbl_comment_replies",
                newName: "tbl_review_replies");

            // 4. Rename column comment_id -> review_id in tbl_review_replies
            migrationBuilder.RenameColumn(
                name: "comment_id",
                table: "tbl_review_replies",
                newName: "review_id");

            // 5. Re-add PK and indexes on tbl_reviews
            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_reviews",
                table: "tbl_reviews",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_reviews_product_id",
                table: "tbl_reviews",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_reviews_user_id_product_id",
                table: "tbl_reviews",
                columns: new[] { "user_id", "product_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_reviews_tbl_products_product_id",
                table: "tbl_reviews",
                column: "product_id",
                principalTable: "tbl_products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_reviews_tbl_users_user_id",
                table: "tbl_reviews",
                column: "user_id",
                principalTable: "tbl_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // 6. Re-add PK, indexes and FK on tbl_review_replies
            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_review_replies",
                table: "tbl_review_replies",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_replies_review_id",
                table: "tbl_review_replies",
                column: "review_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_review_replies_staff_id",
                table: "tbl_review_replies",
                column: "staff_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_review_replies_tbl_reviews_review_id",
                table: "tbl_review_replies",
                column: "review_id",
                principalTable: "tbl_reviews",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_review_replies_tbl_users_staff_id",
                table: "tbl_review_replies",
                column: "staff_id",
                principalTable: "tbl_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // 7. Create new tbl_product_comments table
            migrationBuilder.CreateTable(
                name: "tbl_product_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_product_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_product_comments_tbl_product_comments_parent_id",
                        column: x => x.parent_id,
                        principalTable: "tbl_product_comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_product_comments_tbl_products_product_id",
                        column: x => x.product_id,
                        principalTable: "tbl_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_product_comments_tbl_users_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_comments_parent_id",
                table: "tbl_product_comments",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_comments_product_id",
                table: "tbl_product_comments",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_comments_user_id",
                table: "tbl_product_comments",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new table
            migrationBuilder.DropTable(
                name: "tbl_product_comments");

            // Reverse rename: tbl_review_replies -> tbl_comment_replies
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_review_replies_tbl_reviews_review_id",
                table: "tbl_review_replies");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_review_replies_tbl_users_staff_id",
                table: "tbl_review_replies");

            migrationBuilder.DropIndex(
                name: "IX_tbl_review_replies_review_id",
                table: "tbl_review_replies");

            migrationBuilder.DropIndex(
                name: "IX_tbl_review_replies_staff_id",
                table: "tbl_review_replies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_review_replies",
                table: "tbl_review_replies");

            // Reverse rename: tbl_reviews -> tbl_comments
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_reviews_tbl_products_product_id",
                table: "tbl_reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_reviews_tbl_users_user_id",
                table: "tbl_reviews");

            migrationBuilder.DropIndex(
                name: "IX_tbl_reviews_product_id",
                table: "tbl_reviews");

            migrationBuilder.DropIndex(
                name: "IX_tbl_reviews_user_id_product_id",
                table: "tbl_reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_reviews",
                table: "tbl_reviews");

            migrationBuilder.RenameTable(
                name: "tbl_reviews",
                newName: "tbl_comments");

            migrationBuilder.RenameTable(
                name: "tbl_review_replies",
                newName: "tbl_comment_replies");

            migrationBuilder.RenameColumn(
                name: "review_id",
                table: "tbl_comment_replies",
                newName: "comment_id");

            // Restore tbl_comments PK and indexes
            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_comments",
                table: "tbl_comments",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_comments_product_id",
                table: "tbl_comments",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_comments_user_id_product_id",
                table: "tbl_comments",
                columns: new[] { "user_id", "product_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_comments_tbl_products_product_id",
                table: "tbl_comments",
                column: "product_id",
                principalTable: "tbl_products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_comments_tbl_users_user_id",
                table: "tbl_comments",
                column: "user_id",
                principalTable: "tbl_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Restore tbl_comment_replies PK, indexes and FK
            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_comment_replies",
                table: "tbl_comment_replies",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_comment_replies_comment_id",
                table: "tbl_comment_replies",
                column: "comment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_comment_replies_staff_id",
                table: "tbl_comment_replies",
                column: "staff_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_comment_replies_tbl_comments_comment_id",
                table: "tbl_comment_replies",
                column: "comment_id",
                principalTable: "tbl_comments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_comment_replies_tbl_users_staff_id",
                table: "tbl_comment_replies",
                column: "staff_id",
                principalTable: "tbl_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
