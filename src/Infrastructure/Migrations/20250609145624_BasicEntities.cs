using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDI_Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BasicEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstateProperties_EstatePropertyDescriptions_FeaturedDescrip~",
                table: "EstateProperties");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EstatePropertyDescriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "EstatePropertyDescriptions",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "EstatePropertyDescriptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "EstatePropertyDescriptions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "EstateProperties",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2",
                table: "EstateProperties",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AreaUnit",
                table: "EstateProperties",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "AreaValue",
                table: "EstateProperties",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Bathrooms",
                table: "EstateProperties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Bedrooms",
                table: "EstateProperties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "EstateProperties",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "EstateProperties",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "EstateProperties",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EstateProperties",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "EstateProperties",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MainImageId",
                table: "EstateProperties",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "EstateProperties",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "EstateProperties",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EstateProperties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "EstateProperties",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "EstateProperties",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Visits",
                table: "EstateProperties",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "EstateProperties",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Street = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Street2 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageThreads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastMessageAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageThreads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageThreads_EstateProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "EstateProperties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    AltText = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsMain = table.Column<bool>(type: "boolean", nullable: false),
                    EstatePropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyImages_EstateProperties_EstatePropertyId",
                        column: x => x.EstatePropertyId,
                        principalTable: "EstateProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyMessageLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyMessageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyMessageLogs_EstateProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "EstateProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyVisitLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyVisitLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyVisitLogs_EstateProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "EstateProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Snippet = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InReplyToMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Members_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_MessageThreads_ThreadId",
                        column: x => x.ThreadId,
                        principalTable: "MessageThreads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Messages_InReplyToMessageId",
                        column: x => x.InReplyToMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MessageRecipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    HasBeenRepliedToByRecipient = table.Column<bool>(type: "boolean", nullable: false),
                    IsStarred = table.Column<bool>(type: "boolean", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageRecipients_Members_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageRecipients_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstateProperties_MainImageId",
                table: "EstateProperties",
                column: "MainImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_UserId",
                table: "Members",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageRecipients_MessageId",
                table: "MessageRecipients",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageRecipients_RecipientId_IsRead_IsArchived_IsDeleted",
                table: "MessageRecipients",
                columns: new[] { "RecipientId", "IsRead", "IsArchived", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_MessageRecipients_RecipientId_IsStarred_IsDeleted",
                table: "MessageRecipients",
                columns: new[] { "RecipientId", "IsStarred", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CreatedAtUtc",
                table: "Messages",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_InReplyToMessageId",
                table: "Messages",
                column: "InReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ThreadId",
                table: "Messages",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageThreads_LastMessageAtUtc",
                table: "MessageThreads",
                column: "LastMessageAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_MessageThreads_PropertyId",
                table: "MessageThreads",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_EstatePropertyId",
                table: "PropertyImages",
                column: "EstatePropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyMessageLogs_PropertyId",
                table: "PropertyMessageLogs",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyMessageLogs_SentOnUtc",
                table: "PropertyMessageLogs",
                column: "SentOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyVisitLogs_PropertyId",
                table: "PropertyVisitLogs",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyVisitLogs_VisitedOnUtc",
                table: "PropertyVisitLogs",
                column: "VisitedOnUtc");

            migrationBuilder.AddForeignKey(
                name: "FK_EstateProperties_EstatePropertyDescriptions_FeaturedDescrip~",
                table: "EstateProperties",
                column: "FeaturedDescriptionId",
                principalTable: "EstatePropertyDescriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EstateProperties_PropertyImages_MainImageId",
                table: "EstateProperties",
                column: "MainImageId",
                principalTable: "PropertyImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstateProperties_EstatePropertyDescriptions_FeaturedDescrip~",
                table: "EstateProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_EstateProperties_PropertyImages_MainImageId",
                table: "EstateProperties");

            migrationBuilder.DropTable(
                name: "MessageRecipients");

            migrationBuilder.DropTable(
                name: "PropertyImages");

            migrationBuilder.DropTable(
                name: "PropertyMessageLogs");

            migrationBuilder.DropTable(
                name: "PropertyVisitLogs");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "MessageThreads");

            migrationBuilder.DropIndex(
                name: "IX_EstateProperties_MainImageId",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EstatePropertyDescriptions");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "EstatePropertyDescriptions");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "EstatePropertyDescriptions");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "EstatePropertyDescriptions");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "Address2",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "AreaUnit",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "AreaValue",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "Bathrooms",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "Bedrooms",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "City",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "MainImageId",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "State",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "Visits",
                table: "EstateProperties");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "EstateProperties");

            migrationBuilder.AddForeignKey(
                name: "FK_EstateProperties_EstatePropertyDescriptions_FeaturedDescrip~",
                table: "EstateProperties",
                column: "FeaturedDescriptionId",
                principalTable: "EstatePropertyDescriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
