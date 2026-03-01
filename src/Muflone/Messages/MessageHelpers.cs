using System;

namespace Muflone.Messages
{
	public static class MessageHelpers
	{
		public static Guid GetCorrelationId(IMessage command)
		{
			if (command.UserProperties.TryGetValue(HeadersNames.CorrelationId, out var correlationId)
				&& correlationId != null
				&& Guid.TryParse(correlationId.ToString(), out var result))
			{
				return result;
			}
			return Guid.Empty;
		}

	}
}
