# ktsu.Containers

A high-performance collection library for .NET providing specialized container implementations with focus on performance, memory efficiency, and developer experience.

## 🚀 Features

- **OrderedCollection<T>**: Maintains elements in sorted order with efficient binary search operations
- **OrderedSet<T>**: Unique elements maintained in sorted order with full ISet<T> support
- **RingBuffer<T>**: Fixed-size circular buffer optimized for streaming data scenarios
- **Comprehensive Benchmarking**: World-class performance validation against .NET collections

## 📦 Collections

### OrderedCollection<T>
A collection that maintains elements in sorted order with efficient insertion and search operations.

**Key Features:**
- O(log n) insertion maintaining sort order
- O(log n) search operations via binary search
- Multiple constructors with capacity and comparer support
- Implements ICollection<T>, IReadOnlyCollection<T>, IReadOnlyList<T>

**Best Use Cases:**
- Incremental sorted data building
- Frequent search operations on ordered data
- Scenarios requiring both order and random access

### OrderedSet<T>
A set implementation that maintains unique elements in sorted order.

**Key Features:**
- O(log n) Add/Remove/Contains operations
- Full ISet<T> interface support (Union, Intersection, Except, etc.)
- Sorted enumeration without additional sorting cost
- Memory-efficient list-based implementation

**Best Use Cases:**
- Unique sorted collections
- Set operations with maintained order
- Mathematical set operations with sorted results

### RingBuffer<T>
A fixed-size circular buffer optimized for continuous data streams.

**Key Features:**
- O(1) insertion and access operations
- Automatic wrapping when capacity is reached
- Index-based access to buffer elements
- Resizing and resampling capabilities
- Power-of-two sizing for optimal performance

**Best Use Cases:**
- Streaming data processing
- Fixed-size caches
- Sliding window algorithms
- Audio/signal processing buffers

## 🏆 Performance & Benchmarking

We've implemented a comprehensive benchmarking system using BenchmarkDotNet that validates our implementations against standard .NET collections.

### Benchmark Categories

#### 🎯 Container-Specific Benchmarks
- **OrderedCollection vs List<T> + Sort vs SortedList<T>**
- **OrderedSet vs HashSet<T> vs SortedSet<T>**
- **RingBuffer vs Queue<T> vs List<T> with size limiting**

#### 📊 Standard Collection Baselines
Performance baselines for all major .NET collections:
- List<T>, HashSet<T>, SortedSet<T>
- Dictionary<TKey,TValue>, SortedDictionary<TKey,TValue>
- Queue<T>, Stack<T>
- ConcurrentDictionary<TKey,TValue>, ConcurrentQueue<T>

#### 🔄 Cross-Collection Comparisons
Direct comparisons for common scenarios:
- Building ordered collections (incremental vs bulk)
- Set operations across different implementations
- Memory efficiency patterns
- Real-world usage workflows

#### 📈 Performance Analysis
Advanced scenarios including:
- Scalability testing (1K to 100K+ elements)
- Edge cases and worst-case scenarios
- Memory pressure analysis
- Specialized use cases (sliding windows, priority queues)

### Running Benchmarks

#### Quick Start
```powershell
# Fast development feedback
.\scripts\run-benchmarks.ps1 -Target Quick

# Compare specific containers
.\scripts\run-benchmarks.ps1 -Target OrderedSet -Export Html

# Cross-collection analysis
.\scripts\run-benchmarks.ps1 -Target CrossComparison -Export All

# Full benchmark suite
.\scripts\run-benchmarks.ps1 -Target All -Export Html
```

#### Available Targets
| Target | Description |
|--------|-------------|
| `Quick` | Fast subset for development feedback |
| `OrderedCollection` | OrderedCollection<T> vs alternatives |
| `OrderedSet` | OrderedSet<T> vs alternatives |
| `RingBuffer` | RingBuffer<T> vs alternatives |
| `Standard` | Built-in .NET collection baselines |
| `CrossComparison` | Direct cross-collection comparisons |
| `PerformanceAnalysis` | Scalability and edge case analysis |
| `All` | Complete benchmark suite |

#### Command Line Options
```bash
# Manual benchmark execution
dotnet run --project Containers.Benchmarks --configuration Release

# Filtered benchmarks
dotnet run --project Containers.Benchmarks --configuration Release -- --filter "*Add*"

# Export results
dotnet run --project Containers.Benchmarks --configuration Release -- --exporters html,csv,json
```

## 📋 Installation

```xml
<PackageReference Include="ktsu.Containers" Version="1.0.0" />
```

## 🔧 Usage Examples

### OrderedCollection<T>
```csharp
// Create and populate
var collection = new OrderedCollection<int>();
collection.Add(3);
collection.Add(1);
collection.Add(2);
// Collection automatically maintains order: [1, 2, 3]

// Efficient search
bool contains = collection.Contains(2); // O(log n) binary search

// Access by index
int first = collection[0]; // 1
```

### OrderedSet<T>
```csharp
// Create with initial data
var set = new OrderedSet<int> { 3, 1, 4, 1, 5 };
// Set contains unique elements in order: [1, 3, 4, 5]

// Set operations
var other = new OrderedSet<int> { 4, 5, 6 };
set.UnionWith(other); // [1, 3, 4, 5, 6]
```

### RingBuffer<T>
```csharp
// Create fixed-size buffer
var buffer = new RingBuffer<int>(3);
buffer.PushBack(1);
buffer.PushBack(2);
buffer.PushBack(3);
buffer.PushBack(4); // Overwrites first element
// Buffer contains: [2, 3, 4]

// Index access
int latest = buffer[buffer.Count - 1]; // 4 (most recent)
```

## 🛠️ Development

### Building
```bash
dotnet build
```

### Testing
```bash
dotnet test
```

### Benchmarking
```bash
# Quick performance check
.\scripts\run-benchmarks.ps1 -Target Quick

# Comprehensive analysis
.\scripts\run-benchmarks.ps1 -Target All -Export Html
```

## 📊 Performance Characteristics

### OrderedCollection<T>
- ✅ **Excellent for**: Incremental sorted insertions, frequent searches
- ⚠️ **Consider alternatives for**: Large bulk operations (List<T> + Sort may be faster)
- 🎯 **Sweet spot**: 100-10,000 elements with mixed insert/search operations

### OrderedSet<T>
- ✅ **Excellent for**: Unique sorted collections, set operations with order
- ⚠️ **Consider alternatives for**: Pure membership testing (HashSet<T> is faster)
- 🎯 **Sweet spot**: Mathematical set operations requiring sorted results

### RingBuffer<T>
- ✅ **Excellent for**: Streaming data, fixed-size caches, sliding windows
- ⚠️ **Consider alternatives for**: Dynamic growth requirements
- 🎯 **Sweet spot**: Continuous data streams with occasional random access

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add comprehensive tests
5. Run benchmarks to validate performance
6. Submit a pull request

### Adding New Containers
When adding new containers:
1. Implement the container in `Containers/`
2. Add comprehensive tests in `Containers.Test/`
3. Create benchmark comparisons in `Containers.Benchmarks/`
4. Update documentation

## 📝 License

Licensed under the MIT License. See [LICENSE.md](LICENSE.md) for details.

## 🏗️ Architecture

The library is built with performance and reliability in mind:

- **Zero-allocation operations** where possible
- **Power-of-two sizing** for optimal memory access patterns
- **Binary search algorithms** for O(log n) performance
- **Comprehensive test coverage** ensuring reliability
- **Benchmark-driven development** validating performance claims

## 🎯 Design Principles

1. **Performance First**: Every operation is optimized for speed and memory efficiency
2. **Standard Compliance**: Full compatibility with .NET collection interfaces
3. **Predictable Behavior**: Clear performance characteristics and edge case handling
4. **Developer Experience**: Intuitive APIs with comprehensive documentation
5. **Validation**: Extensive testing and benchmarking ensures reliability

This library provides high-performance alternatives to standard .NET collections while maintaining familiar APIs and adding specialized functionality for common use cases.
