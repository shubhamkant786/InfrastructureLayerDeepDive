using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.SqlServer
{
    public class SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : DbContext(options)
    {
        public DbSet<WeatherEntity> WeatherForecasts => Set<WeatherEntity>();

        public DbSet<LocationEntity> Locations => Set<LocationEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherEntity>(entity =>
            {
                entity.ToTable("WeatherForecasts");
                entity.HasKey(e => e.Date);
                entity.Property(e => e.Summary).HasMaxLength(50);
            });

            modelBuilder.Entity<LocationEntity>(entity =>
            {
                entity.ToTable("Locations");
                entity.HasKey(e => e.LocationId);
                entity.Property(e => e.Point).HasMaxLength(50);
            });
        }
    }
}