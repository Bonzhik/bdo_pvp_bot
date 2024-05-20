using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;

namespace bdo_pvp_bot.Data
{
    public class ApplicationDbContext : DbContext
    {
        private static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(Log.Logger);
        });


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql("User ID = postgres; Password=1234;Server=localhost;Port=5432;Database=BotDb")
                .UseLazyLoadingProxies(true)
                .UseLoggerFactory(MyLoggerFactory)
                .EnableSensitiveDataLogging(false);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<OneVsOneMatch> OneVsOneMatches { get; set; }
        public DbSet<SolareTeam> SolareTeams { get; set; }
        public DbSet<SolareMatch> SolareMatches { get; set; }
        public DbSet<CharacterClass> CharacterClasses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Character>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<Character>()
                .HasMany(c => c.Teams)
                .WithMany(t => t.Characters);

            modelBuilder.Entity<User>()
                .HasOne(u => u.CurrentCharacter)
                .WithMany()
                .HasForeignKey(u => u.CurrentCharacterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SolareMatch>()
                .HasOne(m => m.FirstTeam)
                .WithMany()
                .HasForeignKey(m => m.FirstTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SolareMatch>()
                .HasOne(m => m.SecondTeam)
                .WithMany()
                .HasForeignKey(m => m.SecondTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SolareMatch>()
                .HasOne(m => m.Winner)
                .WithMany()
                .HasForeignKey(m => m.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SolareMatch>()
                .HasOne(m => m.Loser)
                .WithMany()
                .HasForeignKey(m => m.LoserId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
