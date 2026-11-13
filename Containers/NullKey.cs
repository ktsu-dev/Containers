// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers;

/// <summary>
/// Rejects a null key for a map whose key type is unconstrained, naming the parameter the caller passed it as.
/// </summary>
/// <remarks>
/// <see cref="Ensure.NotNull{T}(T, string)"/> takes its name from <c>[CallerArgumentExpression]</c>, which records
/// the whole argument text. At a call site, the <c>(object?)</c> cast an unconstrained key needs would end up in
/// <see cref="ArgumentException.ParamName"/>. Each check here takes the key through a parameter named for what
/// <see cref="Dictionary{TKey, TValue}"/> reports, so the recorded expression is exactly that name.
/// </remarks>
internal static class NullKey
{
	/// <summary>
	/// Throws when a key passed as <c>key</c> is null.
	/// </summary>
	/// <param name="key">The key to check.</param>
	/// <exception cref="ArgumentNullException">Thrown, with a ParamName of <c>key</c>, when <paramref name="key"/> is null.</exception>
	internal static void ThrowIfNull(object? key) => Ensure.NotNull(key);

	/// <summary>
	/// Throws when a key passed as <c>item</c>, as to a key collection's <c>Contains</c>, is null.
	/// </summary>
	/// <param name="item">The key to check.</param>
	/// <exception cref="ArgumentNullException">Thrown, with a ParamName of <c>item</c>, when <paramref name="item"/> is null.</exception>
	internal static void ThrowIfNullItem(object? item) => Ensure.NotNull(item);

	/// <summary>
	/// Throws when a key read from a source dictionary passed as <c>dictionary</c> is null.
	/// </summary>
	/// <param name="dictionary">The key to check.</param>
	/// <exception cref="ArgumentNullException">Thrown, with a ParamName of <c>dictionary</c>, when <paramref name="dictionary"/> is null.</exception>
	internal static void ThrowIfNullIn(object? dictionary) => Ensure.NotNull(dictionary);

	/// <summary>
	/// Throws when a key read from a sequence of pairs passed as <c>pairs</c>, as to <c>AddRange</c>, is null.
	/// </summary>
	/// <param name="pairs">The key to check.</param>
	/// <exception cref="ArgumentNullException">Thrown, with a ParamName of <c>pairs</c>, when <paramref name="pairs"/> is null.</exception>
	internal static void ThrowIfNullInPairs(object? pairs) => Ensure.NotNull(pairs);
}
