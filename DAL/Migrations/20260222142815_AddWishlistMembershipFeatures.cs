using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddWishlistMembershipFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create tables only if they don't exist (for cases where DB already has tables from previous session)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS tbl_linked_accounts (
                    id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    provider character varying(50) NOT NULL,
                    provider_user_id character varying(255) NOT NULL,
                    provider_email character varying(255),
                    provider_name character varying(200),
                    provider_avatar_url character varying(500),
                    linked_at timestamp with time zone NOT NULL,
                    last_used_at timestamp with time zone,
                    CONSTRAINT ""PK_tbl_linked_accounts"" PRIMARY KEY (id),
                    CONSTRAINT ""FK_tbl_linked_accounts_tbl_users_user_id"" FOREIGN KEY (user_id) REFERENCES tbl_users (id) ON DELETE CASCADE
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS tbl_membership_tiers (
                    id uuid NOT NULL,
                    name character varying(50) NOT NULL,
                    min_points integer NOT NULL,
                    max_points integer NOT NULL,
                    discount_percent numeric(5,2) NOT NULL,
                    benefits text,
                    icon_url character varying(500),
                    display_order integer NOT NULL,
                    is_active boolean NOT NULL,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_tbl_membership_tiers"" PRIMARY KEY (id)
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS tbl_point_transactions (
                    id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    points integer NOT NULL,
                    type character varying(50) NOT NULL,
                    order_id uuid,
                    description character varying(500),
                    created_at timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_tbl_point_transactions"" PRIMARY KEY (id),
                    CONSTRAINT ""FK_tbl_point_transactions_tbl_orders_order_id"" FOREIGN KEY (order_id) REFERENCES tbl_orders (id) ON DELETE SET NULL,
                    CONSTRAINT ""FK_tbl_point_transactions_tbl_users_user_id"" FOREIGN KEY (user_id) REFERENCES tbl_users (id) ON DELETE CASCADE
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS tbl_wishlist_items (
                    id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    product_id uuid NOT NULL,
                    created_at timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_tbl_wishlist_items"" PRIMARY KEY (id),
                    CONSTRAINT ""FK_tbl_wishlist_items_tbl_products_product_id"" FOREIGN KEY (product_id) REFERENCES tbl_products (id) ON DELETE CASCADE,
                    CONSTRAINT ""FK_tbl_wishlist_items_tbl_users_user_id"" FOREIGN KEY (user_id) REFERENCES tbl_users (id) ON DELETE CASCADE
                );
            ");

            // Create indexes if not exist
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_tbl_linked_accounts_provider_provider_user_id"" ON tbl_linked_accounts (provider, provider_user_id);");
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_tbl_linked_accounts_user_id_provider"" ON tbl_linked_accounts (user_id, provider);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_tbl_membership_tiers_display_order"" ON tbl_membership_tiers (display_order);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_tbl_membership_tiers_is_active"" ON tbl_membership_tiers (is_active);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_tbl_point_transactions_created_at"" ON tbl_point_transactions (created_at);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_tbl_point_transactions_order_id"" ON tbl_point_transactions (order_id);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_tbl_point_transactions_user_id"" ON tbl_point_transactions (user_id);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_tbl_wishlist_items_product_id"" ON tbl_wishlist_items (product_id);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_tbl_wishlist_items_user_id"" ON tbl_wishlist_items (user_id);");
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_tbl_wishlist_items_user_id_product_id"" ON tbl_wishlist_items (user_id, product_id);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "tbl_linked_accounts");
            migrationBuilder.DropTable(name: "tbl_membership_tiers");
            migrationBuilder.DropTable(name: "tbl_point_transactions");
            migrationBuilder.DropTable(name: "tbl_wishlist_items");
        }
    }
}
