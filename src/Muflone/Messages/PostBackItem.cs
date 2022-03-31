using System.Threading;

namespace Muflone.Messages;

public class PostBackItem
{
	private readonly SendOrPostCallback _sendOrPostCallback;
	private readonly object _postbackState;

	/// <summary>
	/// Creates an instance of PostBackItem
	/// </summary>
	/// <param name="sendOrPostCallback">The callback delegate</param>
	/// <param name="postbackState">The arguments to pass to <see cref="sendOrPostCallback"/></param>
	public PostBackItem(SendOrPostCallback sendOrPostCallback, object postbackState)
	{
		_sendOrPostCallback = sendOrPostCallback;
		_postbackState = postbackState;
	}


	public void Call()
	{
		_sendOrPostCallback(_postbackState);
	}
}