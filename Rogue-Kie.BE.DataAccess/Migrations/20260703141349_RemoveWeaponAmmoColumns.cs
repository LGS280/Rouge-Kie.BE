using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWeaponAmmoColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmmoCapacity",
                table: "WeaponConfigs");

            migrationBuilder.DropColumn(
                name: "ReloadTime",
                table: "WeaponConfigs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$HmaXUDRQHiB1VCRbDlE3VeawNChGPVzAsLU.0vbHNlaVcR/5tb61m");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$H9DVNYu5cp/vVHSaXUw0ju/EYB1yfQkrNPksfJAvxiSFKqttWfvAS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AmmoCapacity",
                table: "WeaponConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "ReloadTime",
                table: "WeaponConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$rVnusOq/UhPJSJ118uvgbeUSMTJMP1Ftpnz3OQLCZCckmUgh0v3Y6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$2BFqDxj09IVDhmDLjclCU.eCXZK5/IcsasrnT/LecuLoqw/P8I67G");
        }
    }
}
