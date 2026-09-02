using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.Oracle
{
    public class OracleWeatherRepository(OracleDbContext dbContext) : IWeatherRepository
    {
        public async Task<IEnumerable<WeatherEntity>> GetAllAsync(CancellationToken cancellationToken)
            => await dbContext.WeatherForecasts.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<WeatherEntity?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken)
            => await dbContext.WeatherForecasts.FindAsync([date], cancellationToken);

        public async Task AddAsync(WeatherEntity entity, CancellationToken cancellationToken)
        {
            await dbContext.WeatherForecasts.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(WeatherEntity entity, CancellationToken cancellationToken)
        {
            dbContext.WeatherForecasts.Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(DateOnly date, CancellationToken cancellationToken)
        {
            var entity = await GetByDateAsync(date, cancellationToken);
            if (entity is not null)
            {
                dbContext.WeatherForecasts.Remove(entity);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}