using Muflone.Core;

namespace Muflone.Messages.Commands
{
  public interface ICommand : IMessage
  {
    IDomainId AggregateId { get; }
  }
}
