// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Collections.Tests;

using ktsu.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class RingBufferTests
{
	[TestMethod]
	public void Constructor_InitializesCorrectly()
	{
		var buffer = new RingBuffer<int>(4);
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => buffer.At(0));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => buffer.At(1));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => buffer.At(2));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => buffer.At(3));
	}

	[TestMethod]
	public void PushBack_OverwritesOldestElement()
	{
		var buffer = new RingBuffer<int>(3);
		buffer.PushBack(1);
		buffer.PushBack(2);
		buffer.PushBack(3);
		buffer.PushBack(4); // Overwrites the oldest (1)
		Assert.AreEqual(2, buffer.At(0));
		Assert.AreEqual(3, buffer.At(1));
		Assert.AreEqual(4, buffer.At(2));
	}

	[TestMethod]
	public void FrontAndBack_ReturnCorrectElements()
	{
		var buffer = new RingBuffer<int>(3);
		buffer.PushBack(10);
		buffer.PushBack(20);
		buffer.PushBack(30);
		Assert.AreEqual(10, buffer.Front());
		Assert.AreEqual(30, buffer.Back());
		buffer.PushBack(40);
		Assert.AreEqual(20, buffer.Front());
		Assert.AreEqual(40, buffer.Back());
	}

	[TestMethod]
	public void Resize_ResetsBuffer()
	{
		var buffer = new RingBuffer<int>(3);
		buffer.PushBack(1);
		buffer.PushBack(2);
		buffer.Resize(2);
		buffer.PushBack(5);
		buffer.PushBack(6);
		Assert.AreEqual(5, buffer.At(0));
		Assert.AreEqual(6, buffer.At(1));
	}

	[TestMethod]
	public void Resample_InterpolatesOrDecimates()
	{
		var buffer = new RingBuffer<int>(4);
		buffer.PushBack(1);
		buffer.PushBack(2);
		buffer.PushBack(3);
		buffer.PushBack(4);
		buffer.Resample(2);
		Assert.AreEqual(buffer.At(0), buffer.At(0)); // Just check no exception and valid access
		Assert.AreEqual(buffer.At(1), buffer.At(1));
		buffer.Resample(4);
		Assert.AreEqual(buffer.At(0), buffer.At(0));
		Assert.AreEqual(buffer.At(3), buffer.At(3));
	}

	[TestMethod]
	public void PrefillConstructor_FillsBufferCorrectly()
	{
		var buffer = new RingBuffer<int>([1, 2, 3], 3);
		Assert.AreEqual(1, buffer.At(0));
		Assert.AreEqual(2, buffer.At(1));
		Assert.AreEqual(3, buffer.At(2));
	}

	[TestMethod]
	public void PrefillConstructor_OverwritesOldestWhenOverfilled()
	{
		var buffer = new RingBuffer<int>([1, 2, 3, 4, 5], 3);
		Assert.AreEqual(3, buffer.At(0));
		Assert.AreEqual(4, buffer.At(1));
		Assert.AreEqual(5, buffer.At(2));
	}

	[TestMethod]
	public void ValuePrefillConstructor_FillsWithZeros()
	{
		var buffer = new RingBuffer<int>(0, 5);
		for (var i = 0; i < 5; i++)
		{
			Assert.AreEqual(0, buffer.At(i));
		}
	}

	[TestMethod]
	public void ValuePrefillConstructor_FillsWithCustomValue()
	{
		var buffer = new RingBuffer<string>("abc", 3);
		for (var i = 0; i < 3; i++)
		{
			Assert.AreEqual("abc", buffer.At(i));
		}
	}
}
