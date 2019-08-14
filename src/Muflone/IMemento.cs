using System;
using Muflone.Core;

namespace Muflone
{
	public interface IMemento
	{
		//TODO Use IDomainId??
		Guid Id { get; set; }
		int Version { get; set; }
	}
}