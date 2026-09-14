using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCharacterAndEnemyConfigSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseDamage",
                table: "EnemyConfigs");

            migrationBuilder.RenameColumn(
                name: "BaseDamage",
                table: "Characters",
                newName: "BaseMana");

            migrationBuilder.AddColumn<int>(
                name: "BaseArmor",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrefabName",
                table: "Characters",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Characters",
                keyColumn: "CharacterId",
                keyValue: 1,
                columns: new[] { "BaseArmor", "BaseMana", "PrefabName" },
                values: new object[] { 4, 200, "Rookie" });

            migrationBuilder.UpdateData(
                table: "Characters",
                keyColumn: "CharacterId",
                keyValue: 2,
                columns: new[] { "BaseArmor", "BaseMana", "PrefabName" },
                values: new object[] { 2, 240, "Zero" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$bSwieoXOWcK5HPl2SvRHSOBgoPBJCSNC8bPwSl5ouP7n.y0sP48Qm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$h3eynoiE8Lhp6bUK4vCTtORbgF034hFM8mTkMQf7do0k4xDAZl9WS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseArmor",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "PrefabName",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "BaseMana",
                table: "Characters",
                newName: "BaseDamage");

            migrationBuilder.AddColumn<int>(
                name: "BaseDamage",
                table: "EnemyConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Characters",
                keyColumn: "CharacterId",
                keyValue: 1,
                column: "BaseDamage",
                value: 15);

            migrationBuilder.UpdateData(
                table: "Characters",
                keyColumn: "CharacterId",
                keyValue: 2,
                column: "BaseDamage",
                value: 25);

            migrationBuilder.UpdateData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "BaseDamage",
                value: 10);

            migrationBuilder.UpdateData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "BaseDamage",
                value: 15);

            migrationBuilder.UpdateData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "BaseDamage",
                value: 30);

            migrationBuilder.UpdateData(
                table: "EnemyConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "BaseDamage",
                value: 100);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$dzQrBpzvlK8oEfw8RnILA.6vl4jGJc8KwH4n3aMhOWjHFctTOBqxe");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$tp90Wr/SOqJ15MMqLyf23uY1Sa2bWqt27tDLpcWOxOZFrNIHGhUTy");
        }
    }
}
