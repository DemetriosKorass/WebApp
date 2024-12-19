using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MigrationDataSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: ["Id", "Name", "Permissions"],
                values: new object[,]
                {
                    { 1, "Default User", 1 },
                    { 2, "Admin", 7 }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: ["Id", "Name"],
                values: new object[,]
                {
                    { 1, "Work" },
                    { 2, "Eat" },
                    { 3, "Sleep" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
