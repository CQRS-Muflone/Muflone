using System.Collections;

namespace Muflone;

public interface ISaga
{
	string Id { get; }

	int Version { get; }

	void Transition(object message);

	ICollection GetUncommittedEvents();

	void ClearUncommittedEvents();

	ICollection GetUndispatchedMessages();

	void ClearUndispatchedMessages();
}