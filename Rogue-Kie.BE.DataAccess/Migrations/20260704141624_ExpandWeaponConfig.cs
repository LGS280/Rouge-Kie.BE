using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ExpandWeaponConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "HandPositionX",
                table: "WeaponConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "HandPositionY",
                table: "WeaponConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "HandPositionZ",
                table: "WeaponConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "PrefabName",
                table: "WeaponConfigs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "RecoilDistance",
                table: "WeaponConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "RecoilDuration",
                table: "WeaponConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "ReturnDuration",
                table: "WeaponConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "ShootSound",
                table: "WeaponConfigs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "ShootVolume",
                table: "WeaponConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$beR2aSvrBiAQYucwhWlAR.SqgRuCV4P3p4PoPpA7Z61LTuSP8KOlu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$nw2coeLSJWL2S/MwiTdQZOJcFrjgtIdoQenLXW6GVFpwFB/35xjA2");

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "HandPositionX", "HandPositionY", "HandPositionZ", "PrefabName", "RecoilDistance", "RecoilDuration", "ReturnDuration", "ShootSound", "ShootVolume" },
                values: new object[] { 0.1f, 0.05f, 0f, "PistolPrefab", 0.1f, 0.05f, 0.1f, "pistol_shoot", 1f });

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "HandPositionX", "HandPositionY", "HandPositionZ", "PrefabName", "RecoilDistance", "RecoilDuration", "ReturnDuration", "ShootSound", "ShootVolume" },
                values: new object[] { 0.2f, 0.1f, 0f, "ShotgunPrefab", 0.3f, 0.1f, 0.2f, "shotgun_shoot", 1.2f });

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "HandPositionX", "HandPositionY", "HandPositionZ", "PrefabName", "RecoilDistance", "RecoilDuration", "ReturnDuration", "ShootSound", "ShootVolume" },
                values: new object[] { 0.3f, 0.15f, 0f, "SniperPrefab", 0.5f, 0.15f, 0.3f, "sniper_shoot", 1.5f });

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "HandPositionX", "HandPositionY", "HandPositionZ", "PrefabName", "RecoilDistance", "RecoilDuration", "ReturnDuration", "ShootSound", "ShootVolume" },
                values: new object[] { 0.2f, 0.1f, 0f, "RiflePrefab", 0.15f, 0.05f, 0.1f, "rifle_shoot", 0.8f });

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "HandPositionX", "HandPositionY", "HandPositionZ", "PrefabName", "RecoilDistance", "RecoilDuration", "ReturnDuration", "ShootSound", "ShootVolume" },
                values: new object[] { 0.1f, 0.2f, 0f, "SwordPrefab", 0f, 0f, 0f, "sword_swing", 1f });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandPositionX",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "HandPositionY",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "HandPositionZ",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "PrefabName",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "RecoilDistance",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "RecoilDuration",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "ReturnDuration",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "ShootSound",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "ShootVolume",
                table: "WeaponConfigs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$0MG13unkvurlI.aW1RPmw.gke3Qz19UCiT9uopk8EtDL8Dd/BgjOy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$arR5Rw/qD8Hw52l3.UjWn.PanHuBhZUacGiV8UUj7zXp2bBBhsOkG");
        }
    }
}
