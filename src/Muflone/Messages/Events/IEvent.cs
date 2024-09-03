namespace Muflone.Messages.Events;

public interface IEvent : IMessage
{
	int Version { get; }
}