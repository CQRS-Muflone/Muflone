using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Messages;

/// <summary>
/// Class MessageConsumer
/// Used to read messages from the broker and Mapping these messages to 
/// A domain-event is usually fire-and-forget, because we do not know it is received.
/// </summary>
public abstract class MessageConsumer<TMessage> where TMessage : class, IMessage
{
	protected readonly IMessageMapper<TMessage> MessageMapper;

	protected MessageConsumer(IMessageMapper<TMessage> messageMapper)
	{
		MessageMapper = messageMapper;
	}

	public abstract Task ConsumeMessagesAsync(TMessage message, CancellationToken cancellationToken = default);
}