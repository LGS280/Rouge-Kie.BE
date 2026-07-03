using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBulletConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Damage",
                table: "WeaponConfigs",
                newName: "ManaCost");

            migrationBuilder.AddColumn<int>(
                name: "BulletId",
                table: "WeaponConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BulletsPerShot",
                table: "WeaponConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "SpreadAngle",
                table: "WeaponConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateTable(
                name: "BulletConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BulletName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Damage = table.Column<int>(type: "integer", nullable: false),
                    CritRate = table.Column<float>(type: "real", nullable: false),
                    FlightSpeed = table.Column<float>(type: "real", nullable: false),
                    PiercingCount = table.Column<int>(type: "integer", nullable: false),
                    PrefabName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulletConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "BulletConfigs",
                columns: new[] { "Id", "BulletName", "CritRate", "Damage", "FlightSpeed", "PiercingCount", "PrefabName" },
                values: new object[,]
                {
                    { 1, "Basic Bullet", 0.1f, 20, 10f, 0, "BasicBulletPrefab" },
                    { 2, "Buckshot", 0.05f, 10, 15f, 0, "BuckshotPrefab" },
                    { 3, "Sniper Round", 0.5f, 150, 30f, 3, "SniperRoundPrefab" },
                    { 4, "Rifle Bullet", 0.2f, 30, 20f, 1, "RifleBulletPrefab" },
                    { 5, "Sword Slash", 0.3f, 40, 5f, 99, "SwordSlashPrefab" }
                });

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

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BulletId", "BulletsPerShot", "ManaCost", "SpreadAngle" },
                values: new object[] { 1, 1, 0, 2f });

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BulletId", "BulletsPerShot", "ManaCost", "SpreadAngle" },
                values: new object[] { 2, 5, 2, 15f });

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BulletId", "BulletsPerShot", "ManaCost", "SpreadAngle" },
                values: new object[] { 3, 1, 5, 0f });

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BulletId", "BulletsPerShot", "ManaCost", "SpreadAngle" },
                values: new object[] { 4, 1, 1, 5f });

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BulletId", "BulletsPerShot", "ManaCost", "SpreadAngle" },
                values: new object[] { 5, 1, 0, 30f });

            migrationBuilder.CreateIndex(
                name: "IX_WeaponConfigs_BulletId",
                table: "WeaponConfigs",
                column: "BulletId");

            migrationBuilder.AddForeignKey(
                name: "FK_WeaponConfigs_BulletConfigs_BulletId",
                table: "WeaponConfigs",
                column: "BulletId",
                principalTable: "BulletConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeaponConfigs_BulletConfigs_BulletId",
                table: "WeaponConfigs");

            migrationBuilder.DropTable(
                name: "BulletConfigs");

            migrationBuilder.DropIndex(
                name: "IX_WeaponConfigs_BulletId",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "BulletId",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "BulletsPerShot",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "SpreadAngle",
                table: "WeaponConfigs");

            migrationBuilder.RenameColumn(
                name: "ManaCost",
                table: "WeaponConfigs",
                newName: "Damage");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$C24MGKhdP9qoPh3GdRNVvO4rUsY5REvy8J/S4C9qrYZ.7ZYf1Q3Si");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$98t77WPOjsGRv69nkgb7NeBJHJsiSJ2EzmvqaImddhoMWPa54Eqe2");

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "Damage",
                value: 20);

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "Damage",
                value: 50);

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "Damage",
                value: 150);

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "Damage",
                value: 30);

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 5,
                column: "Damage",
                value: 40);
        }
    }
}
