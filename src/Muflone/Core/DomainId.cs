using System;

namespace Muflone.Core
{
  public abstract class DomainId : IDomainId, IEquatable<DomainId>
  {
    public Guid Value { get; }

    protected DomainId(Guid value)
    {
      Value = value;
    }

    public override int GetHashCode()
    {
      return Value.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as DomainId);
    }

    public bool Equals(DomainId other)
    {
      return (null != other) && (GetType() == other.GetType()) && (other.Value == Value);
    }

    public static bool operator ==(DomainId domainId1, DomainId domainId2)
    {
      if ((object)domainId1 == null && (object)domainId2 == null)
        return true;

      if ((object)domainId1 == null || (object)domainId2 == null)
        return false;

      return ((domainId1.GetType() == domainId2.GetType()) && (domainId1.Value == domainId2.Value));
    }

    public static bool operator !=(DomainId domainId1, DomainId domainId2)
    {
      return (!(domainId1 == domainId2));
    }

    public override string ToString()
    {
      return Value.ToString();
    }
  }
}
