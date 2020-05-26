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
      get { return Guid.Parse(Standards[HeadersType.CorrelationId]); }
      set { Standards[HeadersType.CorrelationId] = value.ToString(); }
    }

    public string Who
    {
      get { return Standards[HeadersType.Who]; }
      set { Standards[HeadersType.Who] = value; }
    }

    public DateTime When
    {
      get { return DateTime.Parse(Standards[HeadersType.When]); }
      set { Standards[HeadersType.When] = value.ToString("O"); }
    }

    public string AggregateType
    {
      get { return Standards[HeadersType.AggregateType]; }
      set { Standards[HeadersType.AggregateType] = value; }
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