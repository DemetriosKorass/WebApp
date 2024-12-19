using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MigrationRoleConsistencyScripts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert default role if not exists
            migrationBuilder.Sql(@"
            IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Default User')
            BEGIN
                INSERT INTO Roles (Name, Permissions) VALUES ('Default User', 0);
            END
        ");

            // Assign default role to existing users without a role
            migrationBuilder.Sql(@"
            UPDATE Users
            SET RoleId = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Default User')
            WHERE RoleId IS NULL;
        ");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
