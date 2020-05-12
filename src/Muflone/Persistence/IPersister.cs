using System.Threading.Tasks;

namespace Muflone.Persistence
{
  public interface IPersister
  {
    Task<T> GetBy<T>(string id) where T : class;
    Task Insert<T>(T entity) where T : class;
    Task Update<T>(T entity) where T : class;
    Task Delete<T>(T entity) where T : class;
  }
}
