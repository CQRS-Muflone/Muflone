namespace Muflone.Messages.Events;

public interface IIntegrationEventHandler : IMessageHandler
{
}

public interface IIntegrationEventHandler<in T> : IIntegrationEventHandler where T : IIntegrationEvent
{
}