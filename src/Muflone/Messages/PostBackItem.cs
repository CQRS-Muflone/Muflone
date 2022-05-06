using System.Threading;

namespace Muflone.Messages;

public class PostBackItem
{
	private readonly SendOrPostCallback sendOrPostCallback;
	private readonly object postbackState;

	/// <summary>
	/// Creates an instance of PostBackItem
	/// </summary>
	/// <param name="sendOrPostCallback">The callback delegate</param>
	/// <param name="postbackState">The arguments to pass to <see cref="sendOrPostCallback"/></param>
	public PostBackItem(SendOrPostCallback sendOrPostCallback, object postbackState)
	{
		this.sendOrPostCallback = sendOrPostCallback;
		this.postbackState = postbackState;
	}


	public void Call()
	{
		sendOrPostCallback(postbackState);
	}
}