namespace InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts
{
    public interface INotificationService<in TMessage>
    {
        Task SendAsync(TMessage message, CancellationToken cancellationToken);
    }
}