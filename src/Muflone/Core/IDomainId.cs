using System;

namespace Muflone.Core
{
  public interface IDomainId
  {
    Guid Value { get; }
  }
}
