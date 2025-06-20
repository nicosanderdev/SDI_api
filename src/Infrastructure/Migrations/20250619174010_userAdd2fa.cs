using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDI_Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class userAdd2fa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "EstateProperties",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstateProperties_OwnerId",
                table: "EstateProperties",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_EstateProperties_Members_OwnerId",
                table: "EstateProperties",
                column: "OwnerId",
                principalTable: "Members",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstateProperties_Members_OwnerId",
                table: "EstateProperties");

            migrationBuilder.DropIndex(
                name: "IX_EstateProperties_OwnerId",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "EstateProperties");
        }
    }
}
