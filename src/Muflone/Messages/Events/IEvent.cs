using System;
using System.Collections.Generic;
using System.Text;

namespace Muflone.Messages.Events
{
	public interface IEvent : IMessage
	{
		string Who { get; }
		int Version { get; }
	}
}
