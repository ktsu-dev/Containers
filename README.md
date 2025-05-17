# ktsu.Containers

A high-performance, modern C# container library for .NET 8 and .NET 9, featuring a fixed-size circular buffer (ring buffer) implementation.

## Overview

The ktsu.Containers library focuses on providing specialized container types to fill gaps in the standard .NET collection libraries. Each container is designed with specific use cases in mind, balancing performance, memory efficiency, and ease of use.

## Features

- **RingBuffer<T>**: A fixed-size, power-of-two circular buffer for fast, efficient storage and retrieval.
  - Overwrites oldest elements when full
  - Access elements by logical index, front, or back
  - Resizing and resampling support
  - Prefill with a sequence or a single repeated value
  - Implements `IEnumerable<T>`, `IReadOnlyCollection<T>`, and `IReadOnlyList<T>`
- .NET 8 and .NET 9 support
- MIT License

## Installation

Add the NuGet package:

```bash
dotnet add package ktsu.Containers
```

## Usage Examples

### Basic Usage

```csharp
using ktsu.Containers;

// Create a ring buffer of length 3
var buffer = new RingBuffer<int>(3);
buffer.PushBack(1);
buffer.PushBack(2);
buffer.PushBack(3);
buffer.PushBack(4); // Overwrites 1
Console.WriteLine(buffer.At(0)); // 2
Console.WriteLine(buffer.Back()); // 4
```

### Creating Pre-filled Buffers

```csharp
// Prefill with a single value
var zeros = new RingBuffer<int>(0, 5); // [0,0,0,0,0]

// Prefill with a sequence
var prefilled = new RingBuffer<int>(new[] { 1, 2, 3, 4, 5 }, 3); // [3,4,5]
```

### Accessing Elements

```csharp
var buffer = new RingBuffer<int>(new[] { 1, 2, 3 }, 3);

// Access by index (via At method or indexer)
int first = buffer.At(0); // 1
int second = buffer[1];   // 2

// Access first/last elements
int oldest = buffer.Front(); // 1 
int newest = buffer.Back();  // 3
```

### Resizing and Resampling

```csharp
var buffer = new RingBuffer<int>(new[] { 10, 20, 30 }, 3);

// Resize (discards all elements)
buffer.Resize(5);

// Add new elements
buffer.PushBack(100);
buffer.PushBack(200);

// Resample (interpolates values to the new size)
buffer.Resample(4); // Will spread the elements across the new size
```

### Enumeration

```csharp
var buffer = new RingBuffer<int>(new[] { 1, 2, 3, 4, 5 }, 5);

// Use in foreach loop
foreach (var item in buffer)
{
    Console.WriteLine(item);
}

// Use LINQ
var sum = buffer.Sum();
var doubled = buffer.Select(x => x * 2).ToList();
```

### Clearing the Buffer

You can clear all elements from a `RingBuffer<T>` without changing its capacity using the `Clear` method:

```csharp
var buffer = new RingBuffer<int>(3);
buffer.PushBack(1);
buffer.PushBack(2);
buffer.Clear(); // buffer is now empty, Count == 0
```

## Performance Focus

The library emphasizes performance through several optimization techniques:

- **Power-of-two buffer sizes**: Replaces modulo operations with more efficient bitwise AND operations for buffer wrapping
- **Minimal memory allocations**: Operations like PushBack have no allocations in the common case
- **O(1) time complexity**: Key operations like access and insertion maintain constant time regardless of buffer size
- **Zero-copy operations**: Where possible, data is accessed in-place without copying

The RingBuffer implementation is particularly efficient for scenarios that involve:

- Time-series data processing
- Audio/video buffering
- Signal processing
- Rolling window calculations
- Game state history for replay/undo functionality
- Event and log management with fixed history
- Any scenario requiring a fixed-size sliding window over a data stream

## Advanced Usage

### Custom Types

The RingBuffer works with any .NET type:

```csharp
// With reference types
var messageBuffer = new RingBuffer<string>("", 100);
messageBuffer.PushBack("Hello");
messageBuffer.PushBack("World");

// With structs
var pointBuffer = new RingBuffer<Point>(10);
pointBuffer.PushBack(new Point(1, 2));
pointBuffer.PushBack(new Point(3, 4));

// With complex types
var userBuffer = new RingBuffer<User>(5);
userBuffer.PushBack(new User { Id = 1, Name = "Alice" });
```

## License

MIT License. Copyright (c) ktsu.dev
