namespace Muflone.Messages;

/// <summary>
/// Interface IMessageMapper
/// Map between a <see cref="ICommand"/> or an <see cref="IDomainEvent"/> and a <see cref="IMessage"/>. You must implement this for each Command or Message you intend to send over
/// a <a href="http://parlab.eecs.berkeley.edu/wiki/_media/patterns/taskqueue.pdf">Task Queue</a> 
/// </summary>
public interface IMessageMapper
{
}

/// <summary>
/// Interface IMessageMapper
/// Map between a <see cref="ICommand"/> or an <see cref="IDomainEvent"/> and a <see cref="IMessage"/>. You must implement this for each Command or Message you intend to send over
/// a <a href="http://parlab.eecs.berkeley.edu/wiki/_media/patterns/taskqueue.pdf">Task Queue</a> 
/// </summary>
/// <typeparam name="TMessage">The type of the t request.</typeparam>
public interface IMessageMapper<TMessage> : IMessageMapper where TMessage : class, IMessage
{
	/// <summary>
	/// Maps to message.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <returns>Message.</returns>
	Message MapToMessage(TMessage request);

	/// <summary>
	/// Maps to request.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <returns>TMessage.</returns>
	TMessage MapToRequest(Message message);
}