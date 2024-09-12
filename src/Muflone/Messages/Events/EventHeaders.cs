using Muflone.CustomTypes;
using System;
using System.Collections.Generic;

namespace Muflone.Messages.Events;

public sealed class EventHeaders
{
	public IDictionary<string, string> Standards { get; set; }
	public IDictionary<string, string> Customs { get; set; }

	public EventHeaders()
	{
		Standards = new Dictionary<string, string>();
		Customs = new Dictionary<string, string>();
	}

	public Guid CorrelationId
	{
		get => Guid.Parse(Standards[HeadersNames.CorrelationId]);
		set => Standards[HeadersNames.CorrelationId] = value.ToString();
	}

	public Account Who
	{
		get => new Account(Standards[HeadersNames.AccountId],
			Standards[HeadersNames.AccountName]);
		set
		{
			Standards[HeadersNames.AccountId] = value.Id;
			Standards[HeadersNames.AccountName] = value.Name;
		}
	}

	public When When
	{
		get => new(long.Parse(Standards[HeadersNames.When]));
		set => Standards[HeadersNames.When] = value.Value.Ticks.ToString();
	}

	public string AggregateType
	{
		get => Standards[HeadersNames.AggregateType];
		set => Standards[HeadersNames.AggregateType] = value;
	}

	public bool ContainsKey(string key)
	{
		return Standards.ContainsKey(key) || Customs.ContainsKey(key);
	}

	public string Get(string key)
	{
		return Customs[key];
	}

	public void Set(string key, string value)
	{
		Customs[key] = value;
	}
}