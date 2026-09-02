using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using InfrastructureLayerDeepDive.Weather.Application.Mapper;
using InfrastructureLayerDeepDive.Weather.Application.Models;
using InfrastructureLayerDeepDive.Weather.Domain;
using Microsoft.Extensions.Logging;

namespace InfrastructureLayerDeepDive.Weather.Application
{
    public class WeatherService(
        ILogger<WeatherService> logger
        , IWeatherCosmosRepository cosmosRepository
        , IMessageConsumer<WeatherModel> messageConsumer
        , IMessageProducer<WeatherEntity> messageProducer) : IWeatherService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return messageConsumer.StartAsync(ProcessMessageAsync, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return messageConsumer.StopAsync(cancellationToken);
        }

        private async Task ProcessMessageAsync(WeatherModel weatherModel, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Processing weather entity: {weatherEntity}", weatherModel);
                if (weatherModel == null)
                {
                    logger.LogWarning("Received null weather entity. Skipping processing.");
                    return;
                }
                if (weatherModel is WeatherModel)
                {
                    var weatherEntity = weatherModel.ToEntity();
                    if (weatherEntity.Date == DateOnly.FromDateTime(DateTime.Now.Date))
                    {
                        await cosmosRepository.CreateAsync(weatherEntity, cancellationToken);
                    }
                    else
                    {
                        logger.LogWarning("Received weather entity with date {date} which is not today's date. Updating entity processing.", weatherEntity.Date);
                        await cosmosRepository.UpdateAsync(weatherEntity, cancellationToken);
                    }
                    await messageProducer.PublishAsync(weatherEntity, cancellationToken);
                }
                else
                {
                    logger.LogWarning("Received message is not of type WeatherEntity. Skipping processing.");
                }
                logger.LogInformation("Successfully processed weather entity: {weatherModel}", weatherModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing weather entity: {weatherModel}", weatherModel);
            }
        }
    }
}
