using System;
using System.Threading.Tasks;

namespace Muflone.Persistence
{
  public static class RepositoryExtensions
  {
    public static async Task Save(this IRepository repository, IAggregate aggregate, Guid commitId)
    {
      await repository.Save(aggregate, commitId, a => { });
    }
  }
}