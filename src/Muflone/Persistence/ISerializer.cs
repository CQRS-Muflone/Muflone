using System.Threading;
using System.Threading.Tasks;

namespace Muflone.Persistence;

public interface ISerializer
{
    Task<T?> DeserializeAsync<T>(string serializedData, CancellationToken cancellationToken = default) where T : class;
    Task<string> SerializeAsync<T>(T data, CancellationToken cancellationToken = default) where T : class;
}