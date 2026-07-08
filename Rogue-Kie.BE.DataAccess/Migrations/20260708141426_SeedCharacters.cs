using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedCharacters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Characters",
                columns: new[] { "CharacterId", "BaseDamage", "BaseHealth", "CurrencyType", "Description", "Name", "SkillSet", "UnlockPrice" },
                values: new object[,]
                {
                    { 1, 15, 100, "Gold", "A brave warrior from Kie kingdom", "Kie Warrior", "Slash, Shield", 0 },
                    { 2, 25, 80, "Gold", "A master of elements from Kie academy", "Kie Mage", "Fireball, Teleport", 100 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$xtAy4.amGYE6oOyHFicY7ed64K7smJ93PWey/d2oJYhuHFiGFVnKu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$MU3XJAz0d4oguiUzCRerz.X0Fg0qrrb4XRy6VuQUYc.6L8V0ShpO6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Characters",
                keyColumn: "CharacterId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Characters",
                keyColumn: "CharacterId",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$oP3ntfWC5ssrKw0CoMLYY.vEzXg7OaC1mbsgm82zSUNHd97gW5W2K");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$ZeBqto0HyffwXpr.H8eKPeE1YNS6UIZnjW/.f7xC8R4OH6BFJ7TKy");
        }
    }
}
