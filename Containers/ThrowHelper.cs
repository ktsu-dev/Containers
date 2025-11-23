// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides helper methods for throwing common exceptions.
/// This class provides polyfills for methods that were introduced in later .NET versions.
/// </summary>
internal static class ThrowHelper
{
#if NET8_0_OR_GREATER
	/// <summary>
	/// Throws an <see cref="ArgumentNullException"/> if the argument is null.
	/// </summary>
	/// <param name="argument">The argument to check.</param>
	/// <param name="paramName">The name of the parameter.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfNull(
		[NotNull] object? argument,
		[CallerArgumentExpression(nameof(argument))] string? paramName = null
	) => ArgumentNullException.ThrowIfNull(argument, paramName);

	/// <summary>
	/// Throws an <see cref="ArgumentOutOfRangeException"/> if the argument is negative.
	/// </summary>
	/// <param name="value">The value to check.</param>
	/// <param name="paramName">The name of the parameter.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfNegative(
		int value,
		[CallerArgumentExpression(nameof(value))] string? paramName = null
	) => ArgumentOutOfRangeException.ThrowIfNegative(value, paramName);

	/// <summary>
	/// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is greater than the comparison value.
	/// </summary>
	/// <param name="value">The value to check.</param>
	/// <param name="other">The comparison value.</param>
	/// <param name="paramName">The name of the parameter.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfGreaterThan(
		int value,
		int other,
		[CallerArgumentExpression(nameof(value))] string? paramName = null
	) => ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, paramName);

	/// <summary>
	/// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is greater than or equal to the comparison value.
	/// </summary>
	/// <param name="value">The value to check.</param>
	/// <param name="other">The comparison value.</param>
	/// <param name="paramName">The name of the parameter.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfGreaterThanOrEqual(
		int value,
		int other,
		[CallerArgumentExpression(nameof(value))] string? paramName = null
	) => ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, other, paramName);
#else
	/// <summary>
	/// Throws an <see cref="ArgumentNullException"/> if the argument is null.
	/// </summary>
	/// <param name="argument">The argument to check.</param>
	/// <param name="paramName">The name of the parameter.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfNull(
		[NotNull] object? argument,
		[CallerArgumentExpression(nameof(argument))] string? paramName = null
	)
	{
		if (argument is null)
		{
			throw new ArgumentNullException(paramName);
		}
	}

	/// <summary>
	/// Throws an <see cref="ArgumentOutOfRangeException"/> if the argument is negative.
	/// </summary>
	/// <param name="value">The value to check.</param>
	/// <param name="paramName">The name of the parameter.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfNegative(
		int value,
		[CallerArgumentExpression(nameof(value))] string? paramName = null
	)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException(paramName, value, "Value must be non-negative.");
		}
	}

	/// <summary>
	/// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is greater than the comparison value.
	/// </summary>
	/// <param name="value">The value to check.</param>
	/// <param name="other">The comparison value.</param>
	/// <param name="paramName">The name of the parameter.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfGreaterThan(
		int value,
		int other,
		[CallerArgumentExpression(nameof(value))] string? paramName = null
	)
	{
		if (value > other)
		{
			throw new ArgumentOutOfRangeException(
				paramName,
				value,
				$"Value must not be greater than {other}."
			);
		}
	}

	/// <summary>
	/// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is greater than or equal to the comparison value.
	/// </summary>
	/// <param name="value">The value to check.</param>
	/// <param name="other">The comparison value.</param>
	/// <param name="paramName">The name of the parameter.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ThrowIfGreaterThanOrEqual(
		int value,
		int other,
		[CallerArgumentExpression(nameof(value))] string? paramName = null
	)
	{
		if (value >= other)
		{
			throw new ArgumentOutOfRangeException(
				paramName,
				value,
				$"Value must be less than {other}."
			);
		}
	}
#endif
}
