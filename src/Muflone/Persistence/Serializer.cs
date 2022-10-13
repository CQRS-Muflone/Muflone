using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Muflone.Persistence;

public class Serializer : ISerializer
{
	public Task<T> DeserializeAsync<T>(string serializedData, CancellationToken cancellationToken = default)
		where T : class
	{
		var result = JsonConvert.DeserializeObject<T>(serializedData);
		return Task.FromResult(result);
	}

	public Task<string> SerializeAsync<T>(T data, CancellationToken cancellationToken = default) where T : class
	{
		var result = JsonConvert.SerializeObject(data);
		return Task.FromResult(result);
	}
}