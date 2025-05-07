namespace Muflone.Messages.Events;

public interface IIntegrationEventHandlerAsync : IMessageHandler
{
}

public interface IIntegrationEventHandlerAsync<T> : IMessageHandlerAsync<T> where T : IIntegrationEvent
{
}