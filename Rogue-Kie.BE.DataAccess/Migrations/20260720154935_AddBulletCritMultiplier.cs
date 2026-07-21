using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBulletCritMultiplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "CritMultiplier",
                table: "BulletConfigs",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.UpdateData(
                table: "BulletConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "CritMultiplier",
                value: 1.5f);

            migrationBuilder.UpdateData(
                table: "BulletConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "CritMultiplier",
                value: 1.5f);

            migrationBuilder.UpdateData(
                table: "BulletConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "CritMultiplier",
                value: 2f);

            migrationBuilder.UpdateData(
                table: "BulletConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "CritMultiplier",
                value: 1.5f);

            migrationBuilder.UpdateData(
                table: "BulletConfigs",
                keyColumn: "Id",
                keyValue: 5,
                column: "CritMultiplier",
                value: 2f);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CritMultiplier",
                table: "BulletConfigs");

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
    }
}
