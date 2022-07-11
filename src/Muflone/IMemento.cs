using System;

namespace Muflone;

public interface IMemento
{
	Guid Id { get; set; }

	int Version { get; set; }
}