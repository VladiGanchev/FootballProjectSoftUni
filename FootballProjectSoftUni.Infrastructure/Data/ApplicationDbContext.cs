﻿using FootballProjectSoftUni.Infrastructure.Data.Configuration;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FootballProjectSoftUni.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private bool _seedDb;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, bool seed = true)
            : base(options)
        {
            if (Database.IsRelational())
            {
                Database.Migrate();
            }
            else
            {
                Database.EnsureCreated();
            }

            _seedDb = seed;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<Team>()
                .HasOne(t => t.Coach)
                .WithOne(c => c.Team)
                .HasForeignKey<Coach>(c => c.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TournamentCity>()
                .HasOne(x => x.Tournament)
                .WithMany(x => x.TournamentCities)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TournamentCity>()
                .HasOne(x => x.City)
                .WithMany(x => x.CityTournamnets)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TournamentTeam>()
                .HasOne(x => x.Tournament)
                .WithMany(x => x.TournamentTeams)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TournamentTeam>()
                .HasOne(x => x.Team)
                .WithMany(x => x.TeamTournaments)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Team>()
               .HasMany(x => x.Players)
               .WithOne(x => x.Team)
               .HasForeignKey(p => p.TeamId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Tournament>()
               .HasOne(t => t.Referee)
               .WithOne(r => r.Tournament)
               .HasForeignKey<Referee>(r => r.TournamentId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TournamentParticipant>()
                .HasKey(x => new { x.ParticipantId, x.TournamentId });

            builder.Entity<TournamentCity>()
                .HasKey(x => new { x.CityId, x.TournamentId });

            builder.Entity<TournamentTeam>()
                .HasKey(x => new { x.TeamId, x.TournamentId });

            builder.Entity<TournamentParticipant>()
                .HasOne(x => x.Tournament)
                .WithMany(x => x.TournamentParticipants)
               .OnDelete(DeleteBehavior.Restrict);

            if (_seedDb)
            {
                builder.ApplyConfiguration(new CityConfiguration());


                var AdminUser = new ApplicationUser()
                {
                    Id = "600bafb9-a73d-4489-a387-643c2b8ae96c",
                    UserName = "admin@mail.com",
                    NormalizedUserName = "ADMIN@MAIL.COM",
                    Email = "admin@mail.com",
                    NormalizedEmail = "ADMIN@MAIL.COM",
                    FirstName = "Great",
                    LastName = "Admin"
                };

                var hasher = new PasswordHasher<ApplicationUser>();
                AdminUser.PasswordHash = hasher.HashPassword(AdminUser, "admin123");

                builder.Entity<ApplicationUser>().HasData(AdminUser);
            }


            base.OnModelCreating(builder);
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Referee> Referees { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<TournamentCity> TournamentsCities { get; set; }
        public DbSet<TournamentParticipant> TournamentsParticipants { get; set; }
        public DbSet<TournamentTeam> TournamentsTeams { get; set; }
    }
}