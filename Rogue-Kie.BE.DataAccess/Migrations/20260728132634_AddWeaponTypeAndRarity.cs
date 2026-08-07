using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddWeaponTypeAndRarity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Rarity",
                table: "WeaponConfigs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WeaponType",
                table: "WeaponConfigs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$T1339EUj.Aj1X11HrzHxUOm0za0sGj/Toc2vg4c3Gytf7F/8X6gha");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$WkCZ3YyNRQgy9bVunDD6PuyqEyYURaA/DhLfYYsdlo7isx5CwrlQq");

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Rarity", "WeaponType" },
                values: new object[] { "Common", "Pistol" });

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Rarity", "WeaponType" },
                values: new object[] { "Rare", "Shotgun" });

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Rarity", "WeaponType" },
                values: new object[] { "Epic", "Sniper" });

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Rarity", "WeaponType" },
                values: new object[] { "Common", "Rifle" });

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Rarity", "WeaponType" },
                values: new object[] { "Rare", "Melee" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rarity",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "WeaponType",
                table: "WeaponConfigs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$397vdlIiZzhDylytiocuoOk6.rfXsD4bG9V7woYFtInQjnSv1g44e");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$O6twhS.RYq.I.22B0GrWge7QJ.bdp3hD75FhT/lDWOxJ.8SJgoJfa");
        }
    }
}
