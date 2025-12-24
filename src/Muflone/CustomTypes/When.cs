using Newtonsoft.Json;
using System;

namespace Muflone.CustomTypes;

public record When
{
    private readonly long _ticks;

    public DateTime Value => new(_ticks);

    public When(DateTime dateTime)
    {
        _ticks = dateTime.Ticks;
    }

    [JsonConstructor]
    public When(long ticks)
    {
        _ticks = ticks;
    }
}