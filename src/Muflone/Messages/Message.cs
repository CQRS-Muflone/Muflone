using System;
using Muflone.Messages.Enums;

namespace Muflone.Messages;

/// <summary>
/// A message sent over <a href="http://parlab.eecs.berkeley.edu/wiki/_media/patterns/taskqueue.pdf">Task Queue</a> for asynchronous processing of a <see cref="ICommand"/>
/// or <see cref="IMessage"/>
/// </summary>
public class Message : IEquatable<Message>
{
	public const string OriginalMessageIdHeaderName = "x-original-message-id";
	public const string DeliveryTagHeaderName = "DeliveryTag";
	public const string RedeliveredHeaderName = "Redelivered";

	/// <summary>
	/// Gets the header.
	/// </summary>
	/// <value>The header.</value>
	public MessageHeader Header { get; private set; }

	/// <summary>
	/// Gets the body.
	/// </summary>
	/// <value>The body.</value>
	public MessageBody Body { get; private set; }

	/// <summary>
	/// Gets the identifier.
	/// </summary>
	/// <value>The identifier.</value>
	public Guid Id => Header.Id;

	public Message()
	{
		Header = new MessageHeader(messageId: Guid.Empty, topic: string.Empty, messageType: MessageType.MtNone);
		Body = new MessageBody(string.Empty);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Message"/> class.
	/// </summary>
	/// <param name="header">The header.</param>
	/// <param name="body">The body.</param>
	public Message(MessageHeader header, MessageBody body)
	{
		Header = header;
		Body = body;
	}

	public ulong DeliveryTag
	{
		get
		{
			if (Header.Bag.ContainsKey(DeliveryTagHeaderName))
				return (ulong)Header.Bag[DeliveryTagHeaderName];
			return (ulong)0;
		}
		set => Header.Bag[DeliveryTagHeaderName] = value;
	}

	public bool Redelivered
	{
		get
		{
			if (Header.Bag.ContainsKey(RedeliveredHeaderName))
				return (bool)Header.Bag[RedeliveredHeaderName];
			return false;
		}
		set { Header.Bag[RedeliveredHeaderName] = value; }
	}

	public void UpdateHandledCount()
	{
		Header.UpdateHandledCount();
	}

	public bool HandledCountReached(int requeueCount)
	{
		return Header.HandledCount >= requeueCount;
	}

	public void Execute()
	{
		if (Header.MessageType != MessageType.MtCallback)
			throw new InvalidOperationException("You cannot execute a callback unless the message is a callback method");
		//Body.Postback.Call();
	}

	/// <summary>
	/// Indicates whether the current object is equal to another object of the same type.
	/// </summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
	public bool Equals(Message other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Header.Equals(other.Header) && Body.Equals(other.Body);
	}

	/// <summary>
	/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
	/// </summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((Message)obj);
	}

	/// <summary>
	/// Returns a hash code for this instance.
	/// </summary>
	/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
	public override int GetHashCode()
	{
		unchecked
		{
			return ((Header != null ? Header.GetHashCode() : 0) * 397) ^ (Body != null ? Body.GetHashCode() : 0);
		}
	}

	/// <summary>
	/// Implements the ==.
	/// </summary>
	/// <param name="left">The left.</param>
	/// <param name="right">The right.</param>
	/// <returns>The result of the operator.</returns>
	public static bool operator ==(Message left, Message right)
	{
		return Equals(left, right);
	}

	/// <summary>
	/// Implements the !=.
	/// </summary>
	/// <param name="left">The left.</param>
	/// <param name="right">The right.</param>
	/// <returns>The result of the operator.</returns>
	public static bool operator !=(Message left, Message right)
	{
		return !Equals(left, right);
	}
}