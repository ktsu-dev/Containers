# ktsu.Collections

A high-performance, modern C# collection library for .NET 8 and .NET 9, featuring a fixed-size circular buffer (ring buffer) implementation.

## Features

- **RingBuffer<T>**: A fixed-size, power-of-two circular buffer for fast, efficient storage and retrieval.
  - Overwrites oldest elements when full
  - Access elements by logical index, front, or back
  - Resizing and resampling support
  - Prefill with a sequence or a single repeated value
- .NET 8 and .NET 9 support
- MIT License

## Usage

using ktsu.Collections;

// Create a ring buffer of length 3
var buffer = new RingBuffer<int>(3);
buffer.PushBack(1);
buffer.PushBack(2);
buffer.PushBack(3);
buffer.PushBack(4); // Overwrites 1
Console.WriteLine(buffer.At(0)); // 2
Console.WriteLine(buffer.Back()); // 4

// Prefill with a single value
var zeros = new RingBuffer<int>(0, 5); // [0,0,0,0,0]

// Prefill with a sequence
var prefilled = new RingBuffer<int>(new[] { 1, 2, 3, 4, 5 }, 3); // [3,4,5]

## Installation

Add the NuGet package (coming soon):dotnet add package ktsu.Collections

## License

MIT License. Copyright (c) ktsu.dev
