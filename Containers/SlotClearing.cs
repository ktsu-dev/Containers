// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

/// <summary>
/// Decides whether a vacated array slot has to be reset so the garbage collector can reclaim what it held.
/// </summary>
internal static class SlotClearing
{
	/// <summary>
	/// Returns whether a slot of type <typeparamref name="T"/> can keep an object alive, and so must be
	/// cleared when an element is removed or the container is cleared.
	/// </summary>
	/// <remarks>
	/// <c>RuntimeHelpers.IsReferenceOrContainsReferences</c> exists from netstandard2.1 and netcoreapp2.0.
	/// On netstandard2.0 there is no cheap equivalent, so the answer is always true: clearing a slot that
	/// holds no references costs little, while skipping one that does leaks. A plain value-type check is
	/// not enough there, because a struct such as <c>KeyValuePair&lt;int, object&gt;</c> holds a reference.
	/// </remarks>
	/// <typeparam name="T">The slot's element type.</typeparam>
	/// <returns>True when the slot must be cleared.</returns>
	internal static bool IsNeeded<T>() =>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
		RuntimeHelpers.IsReferenceOrContainsReferences<T>();
#else
		true;
#endif
}
