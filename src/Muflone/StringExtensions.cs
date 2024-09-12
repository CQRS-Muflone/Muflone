using System;

namespace Muflone;

internal static class StringExtensions
{
	public static Guid ToGuid(this string value)
	{
		Guid.TryParse(value, out var guid);
		return guid;
	}
}