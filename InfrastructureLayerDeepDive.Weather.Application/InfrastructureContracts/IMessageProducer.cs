namespace InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts
{
    public interface IMessageProducer<in TMessage>
    {
        Task PublishAsync(TMessage message, CancellationToken cancellationToken);
    }
}