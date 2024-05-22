﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using bdo_pvp_bot.Data;

#nullable disable

namespace bdo_pvp_bot.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CharacterSolareTeam", b =>
                {
                    b.Property<long>("CharactersId")
                        .HasColumnType("bigint");

                    b.Property<long>("TeamsId")
                        .HasColumnType("bigint");

                    b.HasKey("CharactersId", "TeamsId");

                    b.HasIndex("TeamsId");

                    b.ToTable("CharacterSolareTeam");
                });

            modelBuilder.Entity("Domain.Entities.Character", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long?>("ClassId")
                        .HasColumnType("bigint");

                    b.Property<int>("Elo")
                        .HasColumnType("integer");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("ClassId");

                    b.HasIndex("UserId");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("Domain.Entities.CharacterClass", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("CharacterClasses");
                });

            modelBuilder.Entity("Domain.Entities.OneVsOneMatch", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long?>("CharacterId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("EndAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("FirstPlayerId")
                        .HasColumnType("bigint");

                    b.Property<long?>("LoserId")
                        .HasColumnType("bigint");

                    b.Property<long>("SecondPlayerId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("StartAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("WinnerId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("CharacterId");

                    b.HasIndex("FirstPlayerId");

                    b.HasIndex("LoserId");

                    b.HasIndex("SecondPlayerId");

                    b.HasIndex("WinnerId");

                    b.ToTable("OneVsOneMatches");
                });

            modelBuilder.Entity("Domain.Entities.SolareMatch", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime?>("EndAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("FirstTeamId")
                        .HasColumnType("bigint");

                    b.Property<long?>("LoserId")
                        .HasColumnType("bigint");

                    b.Property<long>("SecondTeamId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("StartAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("WinnerId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("FirstTeamId");

                    b.HasIndex("LoserId");

                    b.HasIndex("SecondTeamId");

                    b.HasIndex("WinnerId");

                    b.ToTable("SolareMatches");
                });

            modelBuilder.Entity("Domain.Entities.SolareTeam", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.HasKey("Id");

                    b.ToTable("SolareTeams");
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long?>("CurrentCharacterId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("DiscordId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("IsInMatch")
                        .HasColumnType("boolean");

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CurrentCharacterId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("CharacterSolareTeam", b =>
                {
                    b.HasOne("Domain.Entities.Character", null)
                        .WithMany()
                        .HasForeignKey("CharactersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.SolareTeam", null)
                        .WithMany()
                        .HasForeignKey("TeamsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Entities.Character", b =>
                {
                    b.HasOne("Domain.Entities.CharacterClass", "Class")
                        .WithMany("Classes")
                        .HasForeignKey("ClassId");

                    b.HasOne("Domain.Entities.User", "User")
                        .WithMany("Characters")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Class");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.OneVsOneMatch", b =>
                {
                    b.HasOne("Domain.Entities.Character", null)
                        .WithMany("OneVsOneMatches")
                        .HasForeignKey("CharacterId");

                    b.HasOne("Domain.Entities.User", "FirstPlayer")
                        .WithMany()
                        .HasForeignKey("FirstPlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.User", "Loser")
                        .WithMany()
                        .HasForeignKey("LoserId");

                    b.HasOne("Domain.Entities.User", "SecondPlayer")
                        .WithMany()
                        .HasForeignKey("SecondPlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.User", "Winner")
                        .WithMany()
                        .HasForeignKey("WinnerId");

                    b.Navigation("FirstPlayer");

                    b.Navigation("Loser");

                    b.Navigation("SecondPlayer");

                    b.Navigation("Winner");
                });

            modelBuilder.Entity("Domain.Entities.SolareMatch", b =>
                {
                    b.HasOne("Domain.Entities.SolareTeam", "FirstTeam")
                        .WithMany()
                        .HasForeignKey("FirstTeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Domain.Entities.SolareTeam", "Loser")
                        .WithMany()
                        .HasForeignKey("LoserId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Domain.Entities.SolareTeam", "SecondTeam")
                        .WithMany()
                        .HasForeignKey("SecondTeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Domain.Entities.SolareTeam", "Winner")
                        .WithMany()
                        .HasForeignKey("WinnerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("FirstTeam");

                    b.Navigation("Loser");

                    b.Navigation("SecondTeam");

                    b.Navigation("Winner");
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.HasOne("Domain.Entities.Character", "CurrentCharacter")
                        .WithMany()
                        .HasForeignKey("CurrentCharacterId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("CurrentCharacter");
                });

            modelBuilder.Entity("Domain.Entities.Character", b =>
                {
                    b.Navigation("OneVsOneMatches");
                });

            modelBuilder.Entity("Domain.Entities.CharacterClass", b =>
                {
                    b.Navigation("Classes");
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.Navigation("Characters");
                });
#pragma warning restore 612, 618
        }
    }
}
