// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Containers.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Guards the capacity growth shared by the contiguous containers, which used to double past
/// <see cref="int.MaxValue"/> into a zero-length array once they held 2^30 elements (issue #90).
/// </summary>
[TestClass]
public class GrowthTests
{
	[TestMethod]
	public void NextCapacity_EmptyArray_UsesDefault() =>
		Assert.AreEqual(4, Growth.NextCapacity(0, 0, 4));

	[TestMethod]
	public void NextCapacity_DoublesTheLength() =>
		Assert.AreEqual(16, Growth.NextCapacity(8, 0, 4));

	[TestMethod]
	public void NextCapacity_HonoursALargerMinimum() =>
		Assert.AreEqual(100, Growth.NextCapacity(8, 100, 4));

	[TestMethod]
	public void NextCapacity_At2Pow30_ClampsToMaxArrayLength() =>
		Assert.AreEqual(Growth.MaxArrayLength, Growth.NextCapacity(1 << 30, 0, 4));

	[TestMethod]
	public void NextCapacity_AboveHalfOfIntMax_ClampsToMaxArrayLength() =>
		Assert.AreEqual(Growth.MaxArrayLength, Growth.NextCapacity((int.MaxValue / 2) + 1, 0, 4));

	[TestMethod]
	public void NextCapacity_AtMaxArrayLength_Throws() =>
		Assert.ThrowsExactly<InvalidOperationException>(() => Growth.NextCapacity(Growth.MaxArrayLength, 0, 4));

	[TestMethod]
	public void MaxArrayLength_MatchesTheRuntime() =>
		Assert.AreEqual(Growth.MaxArrayLength, Array.MaxLength);
}
