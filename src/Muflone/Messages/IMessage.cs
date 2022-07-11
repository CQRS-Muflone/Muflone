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