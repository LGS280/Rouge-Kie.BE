using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondBulletIdToWeaponConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SecondBulletId",
                table: "WeaponConfigs",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$OxxU43iRst4yzPMA2WWBN./.QXIYhImg9ePK/hs8rTSSxJH/g4Ybi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$W68HxynSoPkLCv7pAoIhVu5S/.tQJpQyZ7RGSXTR8L5D0uFwUTOE2");

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "SecondBulletId",
                value: null);

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "SecondBulletId",
                value: null);

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "SecondBulletId",
                value: null);

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "SecondBulletId",
                value: null);

            migrationBuilder.UpdateData(
                table: "WeaponConfigs",
                keyColumn: "Id",
                keyValue: 5,
                column: "SecondBulletId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_WeaponConfigs_SecondBulletId",
                table: "WeaponConfigs",
                column: "SecondBulletId");

            migrationBuilder.AddForeignKey(
                name: "FK_WeaponConfigs_BulletConfigs_SecondBulletId",
                table: "WeaponConfigs",
                column: "SecondBulletId",
                principalTable: "BulletConfigs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeaponConfigs_BulletConfigs_SecondBulletId",
                table: "WeaponConfigs");

            migrationBuilder.DropIndex(
                name: "IX_WeaponConfigs_SecondBulletId",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "SecondBulletId",
                table: "WeaponConfigs");

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
        }
    }
}
