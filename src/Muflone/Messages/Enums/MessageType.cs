namespace Muflone.Messages.Enums;

/// <summary>
/// Enum MessageType
/// The type of a message, used on the receiving side of a Task Queue to handle the message appropriately
/// </summary>
public enum MessageType
{
	/// <summary>
	/// The message could not be read
	/// </summary>
	MtUnacceptable = -1,

	/// <summary>
	/// The message type has not been configured
	/// </summary>
	MtNone = 0,

	/// <summary>
	/// The message was sent as a command, and the producer intended it to be handled by a consumer
	/// </summary>
	MtCommand = 1,

	/// <summary>
	/// The message was raised as an event and the producer does not care if anyone listens to it
	/// It only contains a simple notification, not the data of what changed
	/// </summary>
	MtEvent = 2,

	/// <summary>
	/// The message was raised as an event and the producer does not care if anyone listens to it
	/// It contains a notification of what changed
	/// </summary>
	MtDocument = 3,

	/// <summary>
	/// A quit message, used to end a dispatcher's message pump
	/// </summary>
	MtQuit = 4,

	/// <summary>
	/// We are a message used to provide a callback for an uncompleted task that has finished and should be scheduled
	/// </summary>
	MtCallback = 5,
}