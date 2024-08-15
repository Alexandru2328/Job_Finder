using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Job_Finder.Migrations
{
    /// <inheritdoc />
    public partial class teste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "271eec0d-6288-4391-962f-3d74d231cdbc");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8d377b2a-0cae-497d-96ed-78d86b2e3524");

            migrationBuilder.AddColumn<int>(
                name: "UserNotificationId",
                table: "Jobs",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2167bd90-fbf7-41a6-b2da-f37117bbbc09", null, "client", null },
                    { "a48aa5b5-3b2c-4475-aa27-0970d96319c5", null, "admin", "client" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_UserNotificationId",
                table: "Jobs",
                column: "UserNotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_UserNotifications_UserNotificationId",
                table: "Jobs",
                column: "UserNotificationId",
                principalTable: "UserNotifications",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_UserNotifications_UserNotificationId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_UserNotificationId",
                table: "Jobs");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2167bd90-fbf7-41a6-b2da-f37117bbbc09");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a48aa5b5-3b2c-4475-aa27-0970d96319c5");

            migrationBuilder.DropColumn(
                name: "UserNotificationId",
                table: "Jobs");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "271eec0d-6288-4391-962f-3d74d231cdbc", null, "admin", "client" },
                    { "8d377b2a-0cae-497d-96ed-78d86b2e3524", null, "client", null }
                });
        }
    }
}
