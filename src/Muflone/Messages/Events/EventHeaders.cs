using System;
using System.Collections.Generic;

namespace Muflone.Messages.Events
{
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
      get { return Guid.Parse(Standards[EventHeadersType.CorrelationId]); }
      set { Standards[EventHeadersType.CorrelationId] = value.ToString(); }
    }

    public string Who
    {
      get { return Standards[EventHeadersType.Who]; }
      set { Standards[EventHeadersType.Who] = value; }
    }

    public DateTime When
    {
      get { return DateTime.Parse(Standards[EventHeadersType.When]); }
      set { Standards[EventHeadersType.When] = value.ToString("O"); }
    }

    public string AggregateType
    {
      get { return Standards[EventHeadersType.AggregateType]; }
      set { Standards[EventHeadersType.AggregateType] = value; }
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
}