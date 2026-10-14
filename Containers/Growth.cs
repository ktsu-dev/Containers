// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers;

/// <summary>
/// Shared capacity growth for the containers that keep their elements in one array.
/// </summary>
internal static class Growth
{
	/// <summary>
	/// The largest array length the runtime allows, the value of <c>Array.MaxLength</c>, which netstandard2.1 lacks.
	/// </summary>
	internal const int MaxArrayLength = 0x7FFFFFC7;

	/// <summary>
	/// Computes the capacity to grow an array to, doubling it as <see cref="List{T}"/> does, but clamped to
	/// <see cref="MaxArrayLength"/> so the doubling cannot overflow.
	/// </summary>
	/// <param name="currentLength">The length of the array being grown.</param>
	/// <param name="minimumCapacity">The minimum capacity required.</param>
	/// <param name="defaultCapacity">The capacity to use when the array is empty.</param>
	/// <returns>The new capacity, which is greater than <paramref name="currentLength"/>.</returns>
	/// <exception cref="InvalidOperationException">The array is already <see cref="MaxArrayLength"/> long.</exception>
	internal static int NextCapacity(int currentLength, int minimumCapacity, int defaultCapacity)
	{
		int newCapacity = currentLength == 0 ? defaultCapacity : (int)Math.Min((long)currentLength * 2, MaxArrayLength);
		if (newCapacity < minimumCapacity)
		{
			newCapacity = minimumCapacity;
		}

		return newCapacity > currentLength
			? newCapacity
			: throw new InvalidOperationException($"The container cannot hold more than {MaxArrayLength} elements.");
	}
}
