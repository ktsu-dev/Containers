// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Enumeration checks shared by every map, which must all behave like <see cref="Dictionary{TKey, TValue}"/>
/// when changed during enumeration.
/// </summary>
internal static class MapEnumerationAssertions
{
	private static IDictionary<int, string> Seeded(Func<IDictionary<int, string>> create, int count)
	{
		IDictionary<int, string> map = create();
		for (int key = 1; key <= count; key++)
		{
			map.Add(key, $"value {key}");
		}

		return map;
	}

	internal static void RemovingDuringForeachThrows(Func<IDictionary<int, string>> create)
	{
		IDictionary<int, string> map = Seeded(create, 4);

		Assert.ThrowsExactly<InvalidOperationException>(() =>
		{
			foreach (KeyValuePair<int, string> pair in map)
			{
				map.Remove(pair.Key);
			}
		});
	}

	internal static void EveryMutatorInvalidatesEnumerators(Func<IDictionary<int, string>> create)
	{
		Action<IDictionary<int, string>>[] mutations =
		[
			m => m.Add(9, "nine"),
			m => m[9] = "nine",
			m => m[1] = "uno",
			m => m.Remove(1),
			m => m.Remove(new KeyValuePair<int, string>(1, "value 1")),
			m => m.Clear(),
		];

		foreach (Action<IDictionary<int, string>> mutate in mutations)
		{
			IDictionary<int, string> map = Seeded(create, 2);
			using IEnumerator<KeyValuePair<int, string>> pairs = map.GetEnumerator();
			using IEnumerator<int> keys = map.Keys.GetEnumerator();
			using IEnumerator<string> values = map.Values.GetEnumerator();
			Assert.IsTrue(pairs.MoveNext());
			Assert.IsTrue(keys.MoveNext());
			Assert.IsTrue(values.MoveNext());

			mutate(map);

			Assert.ThrowsExactly<InvalidOperationException>(() => pairs.MoveNext());
			Assert.ThrowsExactly<InvalidOperationException>(() => keys.MoveNext());
			Assert.ThrowsExactly<InvalidOperationException>(() => values.MoveNext());
		}
	}

	internal static void UnchangedEnumerationVisitsEveryEntry(Func<IDictionary<int, string>> create)
	{
		IDictionary<int, string> map = Seeded(create, 2);

		Assert.AreSequenceEqual([1, 2], map.Keys);
		Assert.AreSequenceEqual(["value 1", "value 2"], map.Values);
	}
}
