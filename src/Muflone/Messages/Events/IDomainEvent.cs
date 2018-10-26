namespace Muflone.Messages.Events
{
  public interface IDomainEvent : IMessage
  {
    string Who { get; }
    int Version { get; }
  }
}
