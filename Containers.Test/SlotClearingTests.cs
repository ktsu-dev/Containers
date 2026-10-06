// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Tests;

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Guards that removed and cleared slots stop keeping their contents alive, including for structs that
/// hold references, which a plain value-type check misses on the netstandard builds (issue #79).
/// </summary>
[TestClass]
public partial class SlotClearingTests
{
	[GeneratedRegex(@"if\s*\(\s*!\s*typeof\([^)]*\)\.IsValueType\s*\)")]
	private static partial Regex ValueTypeGuard();

	[TestMethod]
	public void IsNeeded_IsTrueForReferencesAndStructsHoldingThem()
	{
		Assert.IsTrue(SlotClearing.IsNeeded<object>());
		Assert.IsTrue(SlotClearing.IsNeeded<KeyValuePair<int, object>>());
		Assert.IsTrue(SlotClearing.IsNeeded<(string, int)>());
	}

	[TestMethod]
	public void IsNeeded_IsFalseForUnmanagedTypesWhereTheRuntimeCanTell() =>
		Assert.IsFalse(SlotClearing.IsNeeded<int>());

	[TestMethod]
	public void ContiguousMap_Clear_ReleasesValues()
	{
		ContiguousMap<int, object> map = [];
		WeakReference[] references = FillMap(map, 3);

		map.Clear();

		AssertAllCollected(references);
	}

	[TestMethod]
	public void ContiguousMap_RemovingEveryKey_ReleasesValues()
	{
		ContiguousMap<int, object> map = [];
		WeakReference[] references = FillMap(map, 3);

		for (int i = 0; i < references.Length; i++)
		{
			Assert.IsTrue(map.Remove(i));
		}

		AssertAllCollected(references);
	}

	[TestMethod]
	public void ContiguousCollection_OfReferenceHoldingStructs_Clear_ReleasesValues()
	{
		ContiguousCollection<KeyValuePair<int, object>> collection = [];
		WeakReference[] references = FillCollection(collection, 3);

		collection.Clear();

		AssertAllCollected(references);
	}

	[TestMethod]
	public void ContiguousCollection_OfReferenceHoldingStructs_Remove_ReleasesValue()
	{
		ContiguousCollection<KeyValuePair<int, object>> collection = [];
		WeakReference[] references = FillCollection(collection, 1);

		collection.RemoveAt(0);

		AssertAllCollected(references);
	}

	[TestMethod]
	public void ContiguousSet_OfReferenceHoldingStructs_Clear_ReleasesValues()
	{
		ContiguousSet<KeyValuePair<int, object>> set = [];
		WeakReference[] references = FillSet(set, 3);

		set.Clear();

		AssertAllCollected(references);
	}

	/// <summary>
	/// The test project only runs on net10, where the runtime check is always available, so this guards
	/// the netstandard fallback at the source: every container must go through
	/// <see cref="SlotClearing.IsNeeded{T}"/> rather than test <c>IsValueType</c> itself.
	/// </summary>
	[TestMethod]
	public void Containers_DoNotDecideSlotClearingWithAValueTypeCheck()
	{
		DirectoryInfo? directory = new(AppContext.BaseDirectory);
		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Containers.sln")))
		{
			directory = directory.Parent;
		}

		Assert.IsNotNull(directory, $"Could not locate Containers.sln above '{AppContext.BaseDirectory}'.");

		string[] offenders =
		[
			.. Directory.EnumerateFiles(Path.Combine(directory.FullName, "Containers"), "*.cs")
				.Where(file => ValueTypeGuard().IsMatch(File.ReadAllText(file)))
				.Select(Path.GetFileName)
				.OfType<string>()
		];

		Assert.IsEmpty(offenders, $"Use SlotClearing.IsNeeded<T>() instead of an IsValueType check in: {string.Join(", ", offenders)}");
	}

	private static void AssertAllCollected(WeakReference[] references)
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		Assert.IsFalse(references.Any(r => r.IsAlive));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static WeakReference[] FillMap(ContiguousMap<int, object> map, int count)
	{
		WeakReference[] references = new WeakReference[count];
		for (int i = 0; i < count; i++)
		{
			object value = new();
			map.Add(i, value);
			references[i] = new WeakReference(value);
		}

		return references;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static WeakReference[] FillCollection(ContiguousCollection<KeyValuePair<int, object>> collection, int count)
	{
		WeakReference[] references = new WeakReference[count];
		for (int i = 0; i < count; i++)
		{
			object value = new();
			collection.Add(new KeyValuePair<int, object>(i, value));
			references[i] = new WeakReference(value);
		}

		return references;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static WeakReference[] FillSet(ContiguousSet<KeyValuePair<int, object>> set, int count)
	{
		WeakReference[] references = new WeakReference[count];
		for (int i = 0; i < count; i++)
		{
			object value = new();
			set.Add(new KeyValuePair<int, object>(i, value));
			references[i] = new WeakReference(value);
		}

		return references;
	}
}
