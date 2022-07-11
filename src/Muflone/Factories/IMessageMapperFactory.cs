using Muflone.Messages;

namespace Muflone.Factories;

public interface IMessageMapperFactory
{
	IMessageMapper<T> CreateMessageMapper<T>() where T : class, IMessage;
}