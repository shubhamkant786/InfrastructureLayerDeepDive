namespace InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts
{
    public interface IMessageConsumer<out TMessage>
    {
        Task StartAsync(Func<TMessage, CancellationToken, Task> onMessageReceived, CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}