using System;
using System.Text;
using Muflone.Messages.Enums;
using Newtonsoft.Json;

namespace Muflone.Messages;

public abstract class MessageMapper<T> : IMessageMapper<T>, IMessageMapper
	where T : class, IMessage
{
	public virtual Message MapToMessage(T request)
	{
		return new Message(
			new MessageHeader(request.MessageId, string.Empty, MessageType.MtNone, new Guid?(), (string)null,
				"text/plain"), new MessageBody(JsonConvert.SerializeObject((object)request), "JSON"));
	}

	public virtual T MapToRequest(Message message)
	{
		return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(message.Body.Bytes));
	}
}