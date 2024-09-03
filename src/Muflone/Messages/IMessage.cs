using System;
using System.Collections.Generic;

namespace Muflone.Messages;

/// <summary>
/// Interface IMessage
/// Base class of <see cref="ICommand"/> and <see cref="IDomainEvent"/>. A message that can be handled by the Command Processor/Dispatcher
/// </summary>
public interface IMessage
{
	/// <summary>
	/// Gets or sets the identifier.
	/// </summary>
	/// <value>The identifier.</value>
	Guid MessageId { get; set; }

	Dictionary<string, object> UserProperties { get; set; }
}


//TODO: Valutare di usare l'envelope al posto che le properties nel messaggio. I ServiceBus e il save di EventStore aprono l'enevelope e usano gli headers corretti e non metà giusti e metà nel body del messaggio

//public interface IMessageEnvelope<T> where T: class, IMessage, new()
//{
//	T Message { get; }
//	Dictionary<string, object> UserProperties { get; }
//}

//public abstract class MessaegEnvelope<T> : IMessageEnvelope<T> where T : class, IMessage, new()
//{
//	public T Message { get;  }
//	public Dictionary<string, object> UserProperties { get;  }
//	public string ClassType => typeof(T).FullName;

//	protected MessaegEnvelope(T message, Dictionary<string, object> userProperties )
//	{
//		Message = message;
//		UserProperties = userProperties;

//	}
//}