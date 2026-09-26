// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers;

/// <summary>
/// Checks whether a type can be ordered by <see cref="Comparer{T}.Default"/> when no comparer is supplied.
/// </summary>
internal static class Comparability
{
	/// <summary>
	/// Determines whether <typeparamref name="T"/> has a default ordering.
	/// </summary>
	/// <remarks>
	/// A <see cref="Nullable{T}"/> implements neither comparison interface itself, but <see cref="Comparer{T}.Default"/>
	/// orders it through its underlying type, with null first, so the underlying type is the one checked.
	/// </remarks>
	/// <typeparam name="T">The type to check.</typeparam>
	/// <returns><see langword="true"/> if <typeparamref name="T"/> or its underlying type implements <see cref="IComparable{T}"/> or <see cref="IComparable"/>.</returns>
	internal static bool HasDefaultOrdering<T>()
	{
		if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)) || typeof(IComparable).IsAssignableFrom(typeof(T)))
		{
			return true;
		}

		Type? underlyingType = Nullable.GetUnderlyingType(typeof(T));
		return underlyingType is not null
			&& (typeof(IComparable<>).MakeGenericType(underlyingType).IsAssignableFrom(underlyingType)
				|| typeof(IComparable).IsAssignableFrom(underlyingType));
	}
}
