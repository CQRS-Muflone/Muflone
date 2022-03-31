using System;

namespace Muflone.Persistence;

public interface IConstructSagas
{
	ISaga Build(Type type, string id);
}