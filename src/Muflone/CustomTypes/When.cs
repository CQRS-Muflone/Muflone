using System;

namespace Muflone.CustomTypes;

public record When
{
	private readonly long ticks;
	public DateTime Value => new(ticks);

	public When(DateTime dateTime)
	{
		ticks = dateTime.Ticks;
	}

	public When(long ticks)
	{
		this.ticks = ticks;
	}
}