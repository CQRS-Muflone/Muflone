using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Persistence;

public class Serializer : ISerializer
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        // Add '$type' property to the JSON for all the objects
        TypeNameHandling = TypeNameHandling.Objects
    };

    public Task<T?> DeserializeAsync<T>(string serializedData, CancellationToken cancellationToken = default)
        where T : class
    {
        var result = JsonConvert.DeserializeObject<T>(serializedData, Settings);
        return Task.FromResult(result);
    }

    public Task<string> SerializeAsync<T>(T data, CancellationToken cancellationToken = default) where T : class
    {
        var result = JsonConvert.SerializeObject(data, Settings);
        return Task.FromResult(result);
    }
}