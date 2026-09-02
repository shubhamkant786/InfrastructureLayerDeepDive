using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.Oracle
{
    public class OracleDbContext(DbContextOptions<OracleDbContext> options) : DbContext(options)
    {
        public DbSet<WeatherEntity> WeatherForecasts => Set<WeatherEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherEntity>(entity =>
            {
                entity.ToTable("WEATHER_FORECASTS");
                entity.HasKey(e => e.Date);
                entity.Property(e => e.Summary).HasMaxLength(50);
            });
        }
    }
}