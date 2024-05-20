using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace bdo_pvp_bot.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CharacterClasses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolareTeams",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolareTeams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolareMatches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstTeamId = table.Column<long>(type: "bigint", nullable: false),
                    SecondTeamId = table.Column<long>(type: "bigint", nullable: false),
                    WinnerId = table.Column<long>(type: "bigint", nullable: true),
                    LoserId = table.Column<long>(type: "bigint", nullable: true),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolareMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolareMatches_SolareTeams_FirstTeamId",
                        column: x => x.FirstTeamId,
                        principalTable: "SolareTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolareMatches_SolareTeams_LoserId",
                        column: x => x.LoserId,
                        principalTable: "SolareTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolareMatches_SolareTeams_SecondTeamId",
                        column: x => x.SecondTeamId,
                        principalTable: "SolareTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolareMatches_SolareTeams_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "SolareTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Elo = table.Column<int>(type: "integer", nullable: false),
                    ClassId = table.Column<long>(type: "bigint", nullable: true),
                    ClassType = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_CharacterClasses_ClassId",
                        column: x => x.ClassId,
                        principalTable: "CharacterClasses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CharacterSolareTeam",
                columns: table => new
                {
                    CharactersId = table.Column<long>(type: "bigint", nullable: false),
                    TeamsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSolareTeam", x => new { x.CharactersId, x.TeamsId });
                    table.ForeignKey(
                        name: "FK_CharacterSolareTeam_Characters_CharactersId",
                        column: x => x.CharactersId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterSolareTeam_SolareTeams_TeamsId",
                        column: x => x.TeamsId,
                        principalTable: "SolareTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nickname = table.Column<string>(type: "text", nullable: false),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    IsInMatch = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentCharacterId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Characters_CurrentCharacterId",
                        column: x => x.CurrentCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OneVsOneMatches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstPlayerId = table.Column<long>(type: "bigint", nullable: false),
                    SecondPlayerId = table.Column<long>(type: "bigint", nullable: false),
                    WinnerId = table.Column<long>(type: "bigint", nullable: true),
                    LoserId = table.Column<long>(type: "bigint", nullable: true),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CharacterId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneVsOneMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OneVsOneMatches_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OneVsOneMatches_Users_FirstPlayerId",
                        column: x => x.FirstPlayerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OneVsOneMatches_Users_LoserId",
                        column: x => x.LoserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OneVsOneMatches_Users_SecondPlayerId",
                        column: x => x.SecondPlayerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OneVsOneMatches_Users_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_ClassId",
                table: "Characters",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_UserId",
                table: "Characters",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSolareTeam_TeamsId",
                table: "CharacterSolareTeam",
                column: "TeamsId");

            migrationBuilder.CreateIndex(
                name: "IX_OneVsOneMatches_CharacterId",
                table: "OneVsOneMatches",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_OneVsOneMatches_FirstPlayerId",
                table: "OneVsOneMatches",
                column: "FirstPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_OneVsOneMatches_LoserId",
                table: "OneVsOneMatches",
                column: "LoserId");

            migrationBuilder.CreateIndex(
                name: "IX_OneVsOneMatches_SecondPlayerId",
                table: "OneVsOneMatches",
                column: "SecondPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_OneVsOneMatches_WinnerId",
                table: "OneVsOneMatches",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SolareMatches_FirstTeamId",
                table: "SolareMatches",
                column: "FirstTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SolareMatches_LoserId",
                table: "SolareMatches",
                column: "LoserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolareMatches_SecondTeamId",
                table: "SolareMatches",
                column: "SecondTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SolareMatches_WinnerId",
                table: "SolareMatches",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CurrentCharacterId",
                table: "Users",
                column: "CurrentCharacterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Users_UserId",
                table: "Characters",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_CharacterClasses_ClassId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Users_UserId",
                table: "Characters");

            migrationBuilder.DropTable(
                name: "CharacterSolareTeam");

            migrationBuilder.DropTable(
                name: "OneVsOneMatches");

            migrationBuilder.DropTable(
                name: "SolareMatches");

            migrationBuilder.DropTable(
                name: "SolareTeams");

            migrationBuilder.DropTable(
                name: "CharacterClasses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Characters");
        }
    }
}
