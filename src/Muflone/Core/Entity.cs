﻿using System;

namespace Muflone.Core;

public abstract class Entity : IEquatable<Entity>
{
	public readonly IDomainId Id = default!;

	protected Entity()
	{
	}

	protected Entity(IDomainId id)
	{
		Id = id;
	}

	public override int GetHashCode()
	{
		return Id.Value.GetHashCode();
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as Entity);
	}

	public bool Equals(Entity? other)
	{
		return (null != other) && (GetType() == other.GetType()) && (other.Id.Value == Id.Value);
	}

	public static bool operator ==(Entity? entity1, Entity? entity2)
	{
		if (entity1 is null && entity2 is null)
			return true;

		if (entity1 is null || entity2 is null)
			return false;

		return (entity1.GetType() == entity2.GetType()) && (entity1.Id.Value == entity2.Id.Value);
	}

	public static bool operator !=(Entity? entity1, Entity? entity2)
	{
		return !(entity1 == entity2);
	}
}