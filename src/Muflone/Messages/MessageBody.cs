﻿using System;
using System.Linq;
using System.Text;

namespace Muflone.Messages;

public class MessageBody : IEquatable<MessageBody>
{
	/// <summary>
	/// The message body as a byte array.
	/// </summary>
	public byte[] Bytes { get; private set; }

	/// <summary>
	/// The type of message encoded into Bytes.  A hint for deserialization that 
	/// will be sent with the byte[] to allow 
	/// </summary>
	public string BodyType { get; private set; }

	/// <summary>
	/// The message body as a string - usually used to store the message body as JSON or XML.
	/// </summary>
	/// <value>The value.</value>
	public string Value
	{
		get { return Encoding.UTF8.GetString(Bytes); }
	}

	/// <summary>
	/// The message body as a callback function with state - usually used with a MessagePumpAsync for a continuation that should be
	/// executed on the message pump thread. Intended for internal use to <see cref="Channel"/>
	/// </summary>
	public PostBackItem PostBack { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MessageBody"/> class with a string.  Use Value property to retrieve.
	/// </summary>
	/// <param name="body">The body of the message, usually XML or JSON.</param>
	/// <param name="bodyType">The type of the message, usualy XML or JSON. Defaults to JSON</param>
	public MessageBody(string body, string bodyType = "JSON")
	{
		Bytes = Encoding.UTF8.GetBytes(body);
		BodyType = bodyType;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MessageBody"/> class using a byte array.
	/// </summary>
	/// <param name="body"></param>
	/// <param name="bodyType">Hint for deserilization, the type of message encoded in body</param>
	public MessageBody(byte[] body, string bodyType)
	{
		Bytes = body;
		BodyType = bodyType ?? "JSON";
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MessageBody"/> class.
	/// </summary>
	/// <param name="postBack">The continuation to run</param>
	public MessageBody(PostBackItem postBack)
	{
		PostBack = postBack;
	}

	/// <summary>
	/// Indicates whether the current object is equal to another object of the same type.
	/// </summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
	public bool Equals(MessageBody other)
	{
		return Bytes.SequenceEqual(other.Bytes) && BodyType.Equals(other.BodyType);
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
		return Equals((MessageBody)obj);
	}

	/// <summary>
	/// Returns a hash code for this instance.
	/// </summary>
	/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
	public override int GetHashCode()
	{
		return (Bytes != null ? Bytes.GetHashCode() : 0);
	}

	/// <summary>
	/// Implements the ==.
	/// </summary>
	/// <param name="left">The left.</param>
	/// <param name="right">The right.</param>
	/// <returns>The result of the operator.</returns>
	public static bool operator ==(MessageBody left, MessageBody right)
	{
		return Equals(left, right);
	}

	/// <summary>
	/// Implements the !=.
	/// </summary>
	/// <param name="left">The left.</param>
	/// <param name="right">The right.</param>
	/// <returns>The result of the operator.</returns>
	public static bool operator !=(MessageBody left, MessageBody right)
	{
		return !Equals(left, right);
	}
}