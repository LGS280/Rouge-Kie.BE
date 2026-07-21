using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBuffSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BuffName", "BuffType", "Description", "IconPath", "Value" },
                values: new object[] { "Vitality", "MaxHP", "Increases Max HP by +2 and heals.", "hp_icon", 2f });

            migrationBuilder.UpdateData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BuffName", "BuffType", "Description", "IconPath", "Rarity", "Value" },
                values: new object[] { "Swift Foot", "MoveSpeed", "Increases movement speed by +15%.", "speed_icon", "Common", 1.15f });

            migrationBuilder.UpdateData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BuffName", "BuffType", "Description", "IconPath", "Rarity", "Value" },
                values: new object[] { "Fierce Combat", "Damage", "Increases all weapon damage by +20%.", "damage_icon", "Common", 1.2f });

            migrationBuilder.InsertData(
                table: "BuffConfigs",
                columns: new[] { "Id", "BuffName", "BuffType", "Description", "IconPath", "Rarity", "Value" },
                values: new object[,]
                {
                    { 4, "Magic Focus", "MaxMana", "Increases Max Mana by +50 and refills it.", "mana_icon", "Common", 50f },
                    { 5, "Golden Touch", "CoinMultiplier", "Increases picked up coins by +50%.", "coin_icon", "Common", 1.5f },
                    { 6, "Sharp Focus", "CritChance", "Increases crit rate by +15%.", "crit_icon", "Common", 15f },
                    { 7, "Rapid Fire", "FireRate", "Decreases gun firing cooldown by 20%.", "firerate_icon", "Common", 0.8f },
                    { 8, "Iron Armor", "MaxArmor", "Increases Max Armor by +2 and refills armor.", "armor_icon", "Common", 2f }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$ycdl6ul2M1ccvWEVrJon1.F0VrzcUoWvjn2lnH9E8agmAJFnbeIPW");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$.2s4znJK78cJ.eSsejj2uOmAHBYSgawfSrICzYCHPU.MHzPxYMuKG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.UpdateData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BuffName", "BuffType", "Description", "IconPath", "Value" },
                values: new object[] { "Speed Boost", "StatModifier", "Increases movement speed.", "speed_icon", 1.5f });

            migrationBuilder.UpdateData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BuffName", "BuffType", "Description", "IconPath", "Rarity", "Value" },
                values: new object[] { "Damage Boost", "StatModifier", "Increases damage dealt.", "damage_icon", "Rare", 2f });

            migrationBuilder.UpdateData(
                table: "BuffConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BuffName", "BuffType", "Description", "IconPath", "Rarity", "Value" },
                values: new object[] { "Health Regen", "Utility", "Regenerates health over time.", "regen_icon", "Epic", 10f });

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
    }
}
