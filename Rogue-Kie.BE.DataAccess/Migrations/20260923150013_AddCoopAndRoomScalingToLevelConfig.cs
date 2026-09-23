using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCoopAndRoomScalingToLevelConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BaseRoomCount",
                table: "LevelConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "CoopBossHPMultiplier",
                table: "LevelConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "CoopExtraMobsPerRoom",
                table: "LevelConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CoopExtraRooms",
                table: "LevelConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "CoopMobHPMultiplier",
                table: "LevelConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 1,
                columns: new[] { "BaseRoomCount", "CoopBossHPMultiplier", "CoopExtraMobsPerRoom", "CoopExtraRooms", "CoopMobHPMultiplier" },
                values: new object[] { 7, 0.6f, 1, 2, 0.4f });

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 2,
                columns: new[] { "BaseRoomCount", "CoopBossHPMultiplier", "CoopExtraMobsPerRoom", "CoopExtraRooms", "CoopMobHPMultiplier" },
                values: new object[] { 8, 0.6f, 1, 2, 0.4f });

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 3,
                columns: new[] { "BaseRoomCount", "CoopBossHPMultiplier", "CoopExtraMobsPerRoom", "CoopExtraRooms", "CoopMobHPMultiplier" },
                values: new object[] { 9, 0.6f, 1, 2, 0.4f });

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 4,
                columns: new[] { "BaseRoomCount", "CoopBossHPMultiplier", "CoopExtraMobsPerRoom", "CoopExtraRooms", "CoopMobHPMultiplier" },
                values: new object[] { 10, 0.6f, 1, 2, 0.4f });

            migrationBuilder.UpdateData(
                table: "LevelConfigs",
                keyColumn: "LevelConfigId",
                keyValue: 5,
                columns: new[] { "BaseRoomCount", "CoopBossHPMultiplier", "CoopExtraMobsPerRoom", "CoopExtraRooms", "CoopMobHPMultiplier" },
                values: new object[] { 11, 0.6f, 1, 2, 0.4f });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseRoomCount",
                table: "LevelConfigs");

            migrationBuilder.DropColumn(
                name: "CoopBossHPMultiplier",
                table: "LevelConfigs");

            migrationBuilder.DropColumn(
                name: "CoopExtraMobsPerRoom",
                table: "LevelConfigs");

            migrationBuilder.DropColumn(
                name: "CoopExtraRooms",
                table: "LevelConfigs");

            migrationBuilder.DropColumn(
                name: "CoopMobHPMultiplier",
                table: "LevelConfigs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$Gb/KPabGzEQE74WYZNnLveUlF3zNwUF1IMnr91dJF0OW/ggbs6wxO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$ONOkrT1WWUZkYNZUrCDRFu3PPPT1D5zli.lJJYoyvneluuqdl7DbO");
        }
    }
}
