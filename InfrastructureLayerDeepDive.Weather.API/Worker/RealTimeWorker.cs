using InfrastructureLayerDeepDive.Weather.Application;

namespace InfrastructureLayerDeepDive.Weather.API.Worker
{
    public class RealTimeWorker(
        ILogger<RealTimeWorker> logger
        , IWeatherService weatherService) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("RealTimeWorker running at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await weatherService.StartAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    logger.LogWarning("Worker cancellation requested. Exiting the worker..");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Application service has crashed. Restarting in 10 seconds...");

                    await Task.Delay(10000, stoppingToken);
                }
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
