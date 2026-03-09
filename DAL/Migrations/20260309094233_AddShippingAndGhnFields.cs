using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingAndGhnFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "expected_delivery_time",
                table: "tbl_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ghn_order_code",
                table: "tbl_orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "shipping_fee",
                table: "tbl_orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "shipping_service_id",
                table: "tbl_orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_note",
                table: "tbl_addresses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "district_id",
                table: "tbl_addresses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "tbl_addresses",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "tbl_addresses",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "province_id",
                table: "tbl_addresses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ward_code",
                table: "tbl_addresses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expected_delivery_time",
                table: "tbl_orders");

            migrationBuilder.DropColumn(
                name: "ghn_order_code",
                table: "tbl_orders");

            migrationBuilder.DropColumn(
                name: "shipping_fee",
                table: "tbl_orders");

            migrationBuilder.DropColumn(
                name: "shipping_service_id",
                table: "tbl_orders");

            migrationBuilder.DropColumn(
                name: "address_note",
                table: "tbl_addresses");

            migrationBuilder.DropColumn(
                name: "district_id",
                table: "tbl_addresses");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "tbl_addresses");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "tbl_addresses");

            migrationBuilder.DropColumn(
                name: "province_id",
                table: "tbl_addresses");

            migrationBuilder.DropColumn(
                name: "ward_code",
                table: "tbl_addresses");
        }
    }
}
