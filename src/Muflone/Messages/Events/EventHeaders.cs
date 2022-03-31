using System;
using System.Collections.Generic;
using Muflone.CustomTypes;

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
		get => Guid.Parse(Standards[EventHeadersType.CorrelationId]);
		set => Standards[EventHeadersType.CorrelationId] = value.ToString();
	}

	public Account Who
	{
		get => new Account(Standards[EventHeadersType.AccountId],
			Standards[EventHeadersType.AccountName]);
		set
		{
			Standards[EventHeadersType.AccountId] = value.Id;
			Standards[EventHeadersType.AccountName] = value.Name;
		}
	}

	public When When
	{
		get => new(long.Parse(Standards[EventHeadersType.When]));
		set => Standards[EventHeadersType.When] = value.Value.Ticks.ToString();
	}

	public string AggregateType
	{
		get => Standards[EventHeadersType.AggregateType];
		set => Standards[EventHeadersType.AggregateType] = value;
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