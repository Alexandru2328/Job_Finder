using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Job_Finder.Migrations
{
    /// <inheritdoc />
    public partial class testtare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2167bd90-fbf7-41a6-b2da-f37117bbbc09");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a48aa5b5-3b2c-4475-aa27-0970d96319c5");

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "UserNotifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "UserNotifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "8c42d74d-ede7-4e5e-8535-687c8b190919", null, "admin", "client" },
                    { "91923ff6-9bc4-4a65-8e99-d11e87319c1b", null, "client", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8c42d74d-ede7-4e5e-8535-687c8b190919");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "91923ff6-9bc4-4a65-8e99-d11e87319c1b");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "UserNotifications");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2167bd90-fbf7-41a6-b2da-f37117bbbc09", null, "client", null },
                    { "a48aa5b5-3b2c-4475-aa27-0970d96319c5", null, "admin", "client" }
                });
        }
    }
}
