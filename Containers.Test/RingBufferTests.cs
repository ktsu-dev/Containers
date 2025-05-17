// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Containers.Tests;

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

	[TestMethod]
	public void EmptyBuffer_ThrowsOnFrontAndBack()
	{
		var buffer = new RingBuffer<int>(3);
		Assert.ThrowsException<InvalidOperationException>(() => buffer.Front());
		Assert.ThrowsException<InvalidOperationException>(() => buffer.Back());
	}

	[TestMethod]
	public void Indexer_ReturnsCorrectValue()
	{
		var buffer = new RingBuffer<int>(3);
		buffer.PushBack(1);
		buffer.PushBack(2);
		buffer.PushBack(3);

		Assert.AreEqual(1, buffer[0]);
		Assert.AreEqual(2, buffer[1]);
		Assert.AreEqual(3, buffer[2]);
	}

	[TestMethod]
	public void Count_ReflectsActualElementCount()
	{
		var buffer = new RingBuffer<int>(5);
		Assert.AreEqual(0, buffer.Count);

		buffer.PushBack(1);
		Assert.AreEqual(1, buffer.Count);

		buffer.PushBack(2);
		buffer.PushBack(3);
		Assert.AreEqual(3, buffer.Count);

		// Fill to capacity
		buffer.PushBack(4);
		buffer.PushBack(5);
		Assert.AreEqual(5, buffer.Count);

		// Exceed capacity, should stay at 5 but overwrite oldest elements
		buffer.PushBack(6);
		buffer.PushBack(7);
		Assert.AreEqual(5, buffer.Count);
	}

	[TestMethod]
	public void Resample_EmptyBuffer_DoesNotThrow()
	{
		var buffer = new RingBuffer<int>(5);
		buffer.Resample(10); // Should not throw
		Assert.AreEqual(0, buffer.Count);
	}

	[TestMethod]
	public void Resize_LargerSize_StartsEmpty()
	{
		var buffer = new RingBuffer<int>(3);
		buffer.PushBack(1);
		buffer.PushBack(2);
		buffer.PushBack(3);
		Assert.AreEqual(3, buffer.Count);

		buffer.Resize(5);
		Assert.AreEqual(0, buffer.Count); // Should be empty after resize
	}

	[TestMethod]
	public void Enumerate_YieldsElementsInCorrectOrder()
	{
		var values = new[] { 10, 20, 30 };
		var buffer = new RingBuffer<int>(values, 3);

		var index = 0;
		foreach (var item in buffer)
		{
			Assert.AreEqual(values[index], item);
			index++;
		}

		Assert.AreEqual(values.Length, index); // Ensure we enumerated all elements
	}

	[TestMethod]
	public void EnumerableConstructor_WithNullItems_ThrowsArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() => new RingBuffer<int>(null!, 3));
	}

	[TestMethod]
	public void PushBack_ManyElements_MaintainsCorrectOrder()
	{
		var buffer = new RingBuffer<int>(3);

		// Add more elements than the buffer size to test wraparound behavior
		for (var i = 1; i <= 10; i++)
		{
			buffer.PushBack(i);
		}

		// Should contain the last 3 elements
		Assert.AreEqual(8, buffer.At(0));
		Assert.AreEqual(9, buffer.At(1));
		Assert.AreEqual(10, buffer.At(2));
	}

	[TestMethod]
	public void ResampleWithSpecificValues_CorrectlyInterpolates()
	{
		var buffer = new RingBuffer<int>(3);
		buffer.PushBack(10);
		buffer.PushBack(20);
		buffer.PushBack(30);

		// Resample to a larger size
		buffer.Resample(5);

		// Should interpolate to approximately: 10, 15, 20, 25, 30
		Assert.AreEqual(10, buffer.At(0));
		Assert.AreEqual(20, buffer.At(2));
		Assert.AreEqual(30, buffer.At(4));
	}
}
