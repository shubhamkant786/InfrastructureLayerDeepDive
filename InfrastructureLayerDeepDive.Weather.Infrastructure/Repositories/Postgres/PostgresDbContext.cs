using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.Postgres
{
    public class PostgresDbContext(DbContextOptions<PostgresDbContext> options) : DbContext(options)
    {
        public DbSet<WeatherEntity> WeatherForecasts => Set<WeatherEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherEntity>(entity =>
            {
                entity.ToTable("weather_forecasts");
                entity.HasKey(e => e.Date);
                entity.Property(e => e.Summary).HasMaxLength(50);
            });
        }
    }
}