namespace Muflone.Messages.Events
{
  public interface IIntegrationEvent: IMessage
  {
    string Who { get; }
    int Version { get; }
  }
}
