using System;
using System.Collections.Generic;
using Muflone.Messages.Enums;

namespace Muflone.Messages;

public class MessageHeader : IEquatable<MessageHeader>
{
	public DateTime TimeStamp { get; private set; }

	/// <summary>
	/// Gets the identifier.
	/// </summary>
	/// <value>The identifier.</value>
	public Guid Id { get; private set; }

	/// <summary>
	/// Gets the topic.
	/// </summary>
	/// <value>The topic.</value>
	public string Topic { get; set; }

	/// <summary>
	/// Gets the type of the message. Used for routing the message to a handler
	/// </summary>
	/// <value>The type of the message.</value>
	public MessageType MessageType { get; private set; }

	/// <summary>
	/// Gets the bag.
	/// </summary>
	/// <value>The bag.</value>
	public Dictionary<string, object> Bag { get; private set; }

	/// <summary>
	/// Gets the number of times this message has been seen 
	/// </summary>
	public int HandledCount { get; set; }

	/// <summary>
	/// Gets the number of milliseconds the message was instructed to be delayed for
	/// </summary>
	public int DelayedMilliseconds { get; set; }

	/// <summary>
	/// Gets or sets the correlation identifier. Used when doing Request-Reply instead of Publish-Subscribe,
	/// allows the originator to match responses to requests
	/// </summary>
	/// <value>The correlation identifier.</value>
	public Guid CorrelationId { get; set; }

	/// <summary>
	/// Gets or sets the ContentType used to describe how the message payload
	/// has been serialized.  Default value is text/plain
	/// </summary>
	public string ContentType { get; set; }

	/// <summary>
	/// Gets or sets the reply to topic. Used when doing Request-Reply instead of Publish-Subscribe to identify
	/// the queue that the sender is listening on. Usually a sender listens on a private queue, so that they
	/// do not have to filter replies intended for other listeners.
	/// </summary>
	/// <value>The reply to.</value>
	public string ReplyTo { get; set; }

	public MessageHeader()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MessageHeader"/> class.
	/// </summary>
	/// <param name="messageId">The message identifier.</param>
	/// <param name="topic">The topic.</param>
	/// <param name="messageType">Type of the message.</param>
	/// <param name="correlationId">Used in request-reply to allow the sender to match response to their request</param>
	/// <param name="replyTo">Used for a request-reply message to indicate the private channel to reply to</param>
	/// <param name="contentType">The type of the payload of the message, defaults to tex/plain</param>
	public MessageHeader(Guid messageId, string topic, MessageType messageType, Guid? correlationId = null,
		string replyTo = null, string contentType = "text/plain")
	{
		Id = messageId;
		Topic = topic;
		MessageType = messageType;
		Bag = new Dictionary<string, object>();
		TimeStamp = RoundToSeconds(DateTime.UtcNow);
		HandledCount = 0;
		DelayedMilliseconds = 0;
		CorrelationId = correlationId ?? Guid.Empty;
		ReplyTo = replyTo;
		ContentType = contentType;
	}

	public MessageHeader(Guid messageId, string topic, MessageType messageType, DateTime timeStamp,
		Guid? correlationId = null, string replyTo = null, string contentType = "text/plain")
		: this(messageId, topic, messageType, correlationId, replyTo, contentType)
	{
		TimeStamp = RoundToSeconds(timeStamp);
	}

	public MessageHeader(Guid messageId, string topic, MessageType messageType, DateTime timeStamp, int handledCount,
		int delayedMilliseconds, Guid? correlationId = null, string replyTo = null, string contentType = "text/plain")
		: this(messageId, topic, messageType, timeStamp, correlationId, replyTo, contentType)
	{
		HandledCount = handledCount;
		DelayedMilliseconds = delayedMilliseconds;
	}


	//AMQP spec says:
	// 4.2.5.4 Timestamps
	// Time stamps are held in the 64-bit POSIX time_t format with an
	// accuracy of one second. By using 64 bits we avoid future wraparound
	// issues associated with 31-bit and 32-bit time_t values.
	private DateTime RoundToSeconds(DateTime dateTime)
	{
		return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond), dateTime.Kind);
	}

	/// <summary>
	/// Indicates whether the current object is equal to another object of the same type.
	/// </summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
	public bool Equals(MessageHeader other)
	{
		return Id == other.Id && Topic == other.Topic && MessageType == other.MessageType;
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
		return Equals((MessageHeader)obj);
	}

	/// <summary>
	/// Returns a hash code for this instance.
	/// </summary>
	/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = Id.GetHashCode();
			hashCode = (hashCode * 397) ^ (Topic != null ? Topic.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ (int)MessageType;
			hashCode = (hashCode * 397) ^ TimeStamp.GetHashCode();
			hashCode = (hashCode * 397) ^ HandledCount.GetHashCode();
			hashCode = (hashCode * 397) ^ DelayedMilliseconds.GetHashCode();
			return hashCode;
		}
	}

	/// <summary>
	/// Implements the ==.
	/// </summary>
	/// <param name="left">The left.</param>
	/// <param name="right">The right.</param>
	/// <returns>The result of the operator.</returns>
	public static bool operator ==(MessageHeader left, MessageHeader right)
	{
		return Equals(left, right);
	}

	/// <summary>
	/// Implements the !=.
	/// </summary>
	/// <param name="left">The left.</param>
	/// <param name="right">The right.</param>
	/// <returns>The result of the operator.</returns>
	public static bool operator !=(MessageHeader left, MessageHeader right)
	{
		return !Equals(left, right);
	}

	/// <summary>
	/// Updates the number of times the message has been seen by the dispatcher; used to determine if it is poisioned and should be discarded.
	/// </summary>
	public void UpdateHandledCount()
	{
		HandledCount++;
	}
}