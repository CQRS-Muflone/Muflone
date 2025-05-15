namespace Muflone.Messages.Events;

public interface IIntegrationEventHandlerAsync : IMessageHandlerAsync
{
}

public interface IIntegrationEventHandlerAsync<T> : IIntegrationEventHandlerAsync, IMessageHandlerAsync<T> where T : IIntegrationEvent
{
}