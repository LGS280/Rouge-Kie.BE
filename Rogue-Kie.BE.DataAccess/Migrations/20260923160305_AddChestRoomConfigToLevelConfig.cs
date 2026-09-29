using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddChestRoomConfigToLevelConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChestRoomCount",
                table: "LevelConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CoopExtraChestRooms",
                table: "LevelConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 1,
                columns: new[] { "ChestRoomCount", "CoopExtraChestRooms" },
                values: new object[] { 1, 0 });

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 2,
                columns: new[] { "ChestRoomCount", "CoopExtraChestRooms" },
                values: new object[] { 1, 0 });

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 3,
                columns: new[] { "ChestRoomCount", "CoopExtraChestRooms" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 4,
                columns: new[] { "ChestRoomCount", "CoopExtraChestRooms" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 5,
                columns: new[] { "ChestRoomCount", "CoopExtraChestRooms" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$v54f0cb5O5J3KqZjTgNCK.08NtGhwbCDxg0ByNrm4wc.FSzecw6xK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$8PQzVVGuV0LPDGvE1zz0dukKaSSW.j5GU9n91J44/Jnhl5ReRh2DC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChestRoomCount",
                table: "LevelConfigs");

            migrationBuilder.DropColumn(
                name: "CoopExtraChestRooms",
                table: "LevelConfigs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$DvIPBLxZzaD5Mi44U65Pue0YRdJ8OB1PBFfhYysvoL0OfszqknEKe");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$LsyW2Rl0KylOJgF/Pnin0u8BmK.Ek5HJJbyjHherflqovrjUqvubO");
        }
    }
}
