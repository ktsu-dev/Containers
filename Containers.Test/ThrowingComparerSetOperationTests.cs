// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// A comparer that throws partway through IntersectWith, ExceptWith or SymmetricExceptWith must leave
/// the set as it was, not half compacted with an element duplicated and another lost (#162).
/// </summary>
[TestClass]
public class ThrowingComparerSetOperationTests
{
	private const string Poison = "m";

	// Insertion order for the hash-backed sets; OrderedSet keeps them sorted as a, c, k, m. Either way
	// the poisoned element comes after one that is dropped and one that is kept.
	private static readonly string[] Elements = ["a", "k", Poison, "c"];

	private static readonly string[] KeptByIntersection = ["k", "c"];

	[TestMethod]
	[DataRow("ContiguousSet", "IntersectWith")]
	[DataRow("ContiguousSet", "ExceptWith")]
	[DataRow("ContiguousSet", "SymmetricExceptWith")]
	[DataRow("InsertionOrderSet", "IntersectWith")]
	[DataRow("InsertionOrderSet", "ExceptWith")]
	[DataRow("InsertionOrderSet", "SymmetricExceptWith")]
	[DataRow("OrderedSet", "IntersectWith")]
	[DataRow("OrderedSet", "ExceptWith")]
	public void SetOperation_WhenComparerThrowsPartway_LeavesSetUnchanged(string container, string operation)
	{
		PoisonComparer comparer = new();
		ISet<string> set = Create(container, comparer);
		foreach (string element in Elements)
		{
			set.Add(element);
		}

		string[] before = [.. set];

		// "a" is dropped (or kept, for ExceptWith) and "k" the other way round before "m" throws
		string[] other = operation == "ExceptWith" ? ["a"] : ["k"];
		comparer.Armed = true;

		Assert.ThrowsExactly<InvalidOperationException>(() => Apply(set, operation, other));

		comparer.Armed = false;
		CollectionAssert.AreEqual(before, set.ToArray(), $"{container}.{operation} changed the set before the comparer threw");
		Assert.AreEqual(before.Length, set.Count);
		foreach (string element in Elements)
		{
			Assert.IsTrue(set.Contains(element), $"{container} lost {element} from its index");
		}

		Assert.IsFalse(set.Add("a"), $"{container} accepted a second \"a\"");
		Assert.AreEqual(before.Length, set.Count);
	}

	[TestMethod]
	[DataRow("ContiguousSet")]
	[DataRow("InsertionOrderSet")]
	[DataRow("OrderedSet")]
	public void IntersectWith_WithAWellBehavedComparer_StillRemovesTheRest(string container)
	{
		ISet<string> set = Create(container, new PoisonComparer());
		foreach (string element in Elements)
		{
			set.Add(element);
		}

		set.IntersectWith(["k", "c", "z"]);

		CollectionAssert.AreEquivalent(KeptByIntersection, set.ToArray());
		Assert.AreEqual(2, set.Count);
		Assert.IsFalse(set.Contains("a"));
		Assert.IsTrue(set.Add("a"));
	}

	private static ISet<string> Create(string container, PoisonComparer comparer) => container switch
	{
		"ContiguousSet" => new ContiguousSet<string>(comparer),
		"InsertionOrderSet" => new InsertionOrderSet<string>(comparer),
		"OrderedSet" => new OrderedSet<string>(comparer),
		_ => throw new ArgumentOutOfRangeException(nameof(container), container, null),
	};

	private static void Apply(ISet<string> set, string operation, string[] other)
	{
		switch (operation)
		{
			case "IntersectWith":
				set.IntersectWith(other);
				break;
			case "ExceptWith":
				set.ExceptWith(other);
				break;
			case "SymmetricExceptWith":
				set.SymmetricExceptWith(other);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
		}
	}

	/// <summary>
	/// Ordinal string comparison that, once armed, throws for any call involving <see cref="Poison"/>.
	/// </summary>
	private sealed class PoisonComparer : IEqualityComparer<string>, IComparer<string>
	{
		public bool Armed { get; set; }

		public bool Equals(string? x, string? y)
		{
			ThrowIfPoisoned(x, y);
			return string.Equals(x, y, StringComparison.Ordinal);
		}

		public int GetHashCode(string obj)
		{
			ThrowIfPoisoned(obj, null);
			return StringComparer.Ordinal.GetHashCode(obj);
		}

		public int Compare(string? x, string? y)
		{
			ThrowIfPoisoned(x, y);
			return string.CompareOrdinal(x, y);
		}

		private void ThrowIfPoisoned(string? x, string? y)
		{
			if (Armed && (x == Poison || y == Poison))
			{
				throw new InvalidOperationException("The comparer refused the poisoned element");
			}
		}
	}
}
