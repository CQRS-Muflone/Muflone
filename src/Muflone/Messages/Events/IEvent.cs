namespace Muflone.Messages.Events
{
	public interface IEvent : IMessage
	{
		string Who { get; }
		int Version { get; }
	}
}
