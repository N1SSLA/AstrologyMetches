using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstrologyMatches.Profiles.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialProfilePhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "profiles");

            migrationBuilder.CreateTable(
                name: "profiles",
                schema: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "profile_photos",
                schema: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    is_main = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profile_photos", x => x.id);
                    table.ForeignKey(
                        name: "fk_profile_photos_profiles_profile_id",
                        column: x => x.profile_id,
                        principalSchema: "profiles",
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_profile_photos_profile_id",
                schema: "profiles",
                table: "profile_photos",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_profiles_user_id",
                schema: "profiles",
                table: "profiles",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profile_photos",
                schema: "profiles");

            migrationBuilder.DropTable(
                name: "profiles",
                schema: "profiles");
        }
    }
}
