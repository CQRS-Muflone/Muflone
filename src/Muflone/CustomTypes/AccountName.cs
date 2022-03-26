using System.Globalization;

namespace Muflone.CustomTypes;

public record AccountName
{
    public string Value { get; init; }

    public AccountName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            value = "Anonymous";

        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}