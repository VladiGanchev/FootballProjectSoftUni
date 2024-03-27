﻿// <auto-generated />
using System;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240326212232_ThirdMigration")]
    partial class ThirdMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.22")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.City", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Cities");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            ImageUrl = "",
                            Name = "Благоевград"
                        },
                        new
                        {
                            Id = 2,
                            ImageUrl = "",
                            Name = "Бургас"
                        },
                        new
                        {
                            Id = 3,
                            ImageUrl = "",
                            Name = "Варна"
                        },
                        new
                        {
                            Id = 4,
                            ImageUrl = "",
                            Name = "Велико Търново"
                        },
                        new
                        {
                            Id = 5,
                            ImageUrl = "",
                            Name = "Видин"
                        },
                        new
                        {
                            Id = 6,
                            ImageUrl = "",
                            Name = "Враца"
                        },
                        new
                        {
                            Id = 7,
                            ImageUrl = "",
                            Name = "Габрово"
                        },
                        new
                        {
                            Id = 8,
                            ImageUrl = "",
                            Name = "Добрич"
                        },
                        new
                        {
                            Id = 9,
                            ImageUrl = "",
                            Name = "Кърджали"
                        },
                        new
                        {
                            Id = 10,
                            ImageUrl = "",
                            Name = "Кюстендил"
                        },
                        new
                        {
                            Id = 11,
                            ImageUrl = "",
                            Name = "Ловеч"
                        },
                        new
                        {
                            Id = 12,
                            ImageUrl = "",
                            Name = "Монтана"
                        },
                        new
                        {
                            Id = 13,
                            ImageUrl = "",
                            Name = "Пазарджик"
                        },
                        new
                        {
                            Id = 14,
                            ImageUrl = "",
                            Name = "Плевен"
                        },
                        new
                        {
                            Id = 15,
                            ImageUrl = "",
                            Name = "Перник"
                        },
                        new
                        {
                            Id = 16,
                            ImageUrl = "",
                            Name = "Пловдив"
                        },
                        new
                        {
                            Id = 17,
                            ImageUrl = "",
                            Name = "Разград"
                        },
                        new
                        {
                            Id = 18,
                            ImageUrl = "",
                            Name = "Русе"
                        },
                        new
                        {
                            Id = 19,
                            ImageUrl = "",
                            Name = "Силистра"
                        },
                        new
                        {
                            Id = 20,
                            ImageUrl = "",
                            Name = "Сливен"
                        },
                        new
                        {
                            Id = 21,
                            ImageUrl = "",
                            Name = "Смолян"
                        },
                        new
                        {
                            Id = 22,
                            ImageUrl = "",
                            Name = "София"
                        },
                        new
                        {
                            Id = 23,
                            ImageUrl = "",
                            Name = "Стара Загора"
                        },
                        new
                        {
                            Id = 24,
                            ImageUrl = "",
                            Name = "Търговище"
                        },
                        new
                        {
                            Id = 25,
                            ImageUrl = "",
                            Name = "Хасково"
                        },
                        new
                        {
                            Id = 26,
                            ImageUrl = "",
                            Name = "Шумен"
                        },
                        new
                        {
                            Id = 27,
                            ImageUrl = "",
                            Name = "Ямбол"
                        });
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.Coach", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Experience")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TeamId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TeamId")
                        .IsUnique()
                        .HasFilter("[TeamId] IS NOT NULL");

                    b.ToTable("Coaches");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TeamId")
                        .IsRequired()
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TeamId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.Referee", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("Birthdate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Experience")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TournamentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TournamentId")
                        .IsUnique();

                    b.ToTable("Referees");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("CoachId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.HasKey("Id");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.Tournament", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NumberOfTeams")
                        .HasColumnType("int");

                    b.Property<string>("OrganiserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RefereeId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OrganiserId");

                    b.ToTable("Tournaments");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.TournamentCity", b =>
                {
                    b.Property<int>("CityId")
                        .HasColumnType("int");

                    b.Property<int>("TournamentId")
                        .HasColumnType("int");

                    b.HasKey("CityId", "TournamentId");

                    b.HasIndex("TournamentId");

                    b.ToTable("TournamentsCities");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.TournamentParticipant", b =>
                {
                    b.Property<string>("ParticipantId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("TournamentId")
                        .HasColumnType("int");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ParticipantId", "TournamentId");

                    b.HasIndex("TournamentId");

                    b.ToTable("TournamentsParticipants");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.TournamentTeam", b =>
                {
                    b.Property<int>("TeamId")
                        .HasColumnType("int");

                    b.Property<int>("TournamentId")
                        .HasColumnType("int");

                    b.HasKey("TeamId", "TournamentId");

                    b.HasIndex("TournamentId");

                    b.ToTable("TournamentsTeams");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.Coach", b =>
                {
                    b.HasOne("FootballProjectSoftUni.Infrastructure.Data.Models.Team", "Team")
                        .WithOne("Coach")
                        .HasForeignKey("FootballProjectSoftUni.Infrastructure.Data.Models.Coach", "TeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Team");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.Player", b =>
                {
                    b.HasOne("FootballProjectSoftUni.Infrastructure.Data.Models.Team", "Team")
                        .WithMany("Players")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Team");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.Referee", b =>
                {
                    b.HasOne("FootballProjectSoftUni.Infrastructure.Data.Models.Tournament", "Tournament")
                        .WithOne("Referee")
                        .HasForeignKey("FootballProjectSoftUni.Infrastructure.Data.Models.Referee", "TournamentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Tournament");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.Tournament", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", "Organiser")
                        .WithMany()
                        .HasForeignKey("OrganiserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organiser");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.TournamentCity", b =>
                {
                    b.HasOne("FootballProjectSoftUni.Infrastructure.Data.Models.City", "City")
                        .WithMany("CityTournamnets")
                        .HasForeignKey("CityId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("FootballProjectSoftUni.Infrastructure.Data.Models.Tournament", "Tournament")
                        .WithMany("TournamentCities")
                        .HasForeignKey("TournamentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("City");

                    b.Navigation("Tournament");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.TournamentParticipant", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", "Participant")
                        .WithMany()
                        .HasForeignKey("ParticipantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FootballProjectSoftUni.Infrastructure.Data.Models.Tournament", "Tournament")
                        .WithMany("TournamentParticipants")
                        .HasForeignKey("TournamentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Participant");

                    b.Navigation("Tournament");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.TournamentTeam", b =>
                {
                    b.HasOne("FootballProjectSoftUni.Infrastructure.Data.Models.Team", "Team")
                        .WithMany("TeamTournaments")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("FootballProjectSoftUni.Infrastructure.Data.Models.Tournament", "Tournament")
                        .WithMany("TournamentTeams")
                        .HasForeignKey("TournamentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Team");

                    b.Navigation("Tournament");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.City", b =>
                {
                    b.Navigation("CityTournamnets");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.Team", b =>
                {
                    b.Navigation("Coach")
                        .IsRequired();

                    b.Navigation("Players");

                    b.Navigation("TeamTournaments");
                });

            modelBuilder.Entity("FootballProjectSoftUni.Infrastructure.Data.Models.Tournament", b =>
                {
                    b.Navigation("Referee");

                    b.Navigation("TournamentCities");

                    b.Navigation("TournamentParticipants");

                    b.Navigation("TournamentTeams");
                });
#pragma warning restore 612, 618
        }
    }
}