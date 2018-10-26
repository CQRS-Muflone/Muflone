using System;
using Muflone.Core;

namespace Muflone.Messages.Commands
{
  [Serializable]
  public class Command : ICommand
  {
    public IDomainId AggregateId { get; set; }
    public Guid CommitId { get; }
    public DateTime CommitDate { get; }

    protected Command(IDomainId aggregateId)
    {
      AggregateId = aggregateId;
      CommitId = Guid.NewGuid();
      CommitDate = DateTime.UtcNow;
    }
  }
}
