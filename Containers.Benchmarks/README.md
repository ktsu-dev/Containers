# ktsu.Containers Benchmarks

This project contains comprehensive performance benchmarks for the ktsu.Containers library using BenchmarkDotNet.

## Overview

The benchmarks compare our custom container implementations against built-in .NET collections to measure:

- **Performance**: Execution time for various operations
- **Memory Usage**: Allocation patterns and memory efficiency
- **Scalability**: How performance changes with different data sizes

## Benchmark Categories

### OrderedCollection Benchmarks
Compares `OrderedCollection<T>` against:
- `List<T>` with manual sorting
- `SortedList<TKey, TValue>`

**Operations tested:**
- Adding elements while maintaining order
- Searching/Contains operations
- Enumeration performance
- Element removal
- Mixed operation scenarios

### OrderedSet Benchmarks
Compares `OrderedSet<T>` against:
- `HashSet<T>` (unordered)
- `SortedSet<T>` (ordered)

**Operations tested:**
- Adding unique elements
- Set operations (Union, Intersection, Except)
- Contains/membership testing
- Enumeration in sorted order
- Mixed operation workflows

### RingBuffer Benchmarks
Compares `RingBuffer<T>` against:
- `Queue<T>` with size limiting
- `List<T>` with circular behavior simulation

**Operations tested:**
- Circular buffer operations
- Index-based access vs enumeration
- Memory allocation patterns
- Resizing and resampling operations
- Continuous data streaming scenarios

## Running Benchmarks

### Run All Benchmarks
```bash
dotnet run --project Containers.Benchmarks --configuration Release
```

### Run Specific Benchmark Class
```bash
dotnet run --project Containers.Benchmarks --configuration Release -- --filter "*OrderedCollection*"
dotnet run --project Containers.Benchmarks --configuration Release -- --filter "*OrderedSet*"
dotnet run --project Containers.Benchmarks --configuration Release -- --filter "*RingBuffer*"
```

### Run Specific Methods
```bash
dotnet run --project Containers.Benchmarks --configuration Release -- --filter "*Add*"
dotnet run --project Containers.Benchmarks --configuration Release -- --filter "*Contains*"
```

### Export Results
```bash
# Export to various formats
dotnet run --project Containers.Benchmarks --configuration Release -- --exporters html
dotnet run --project Containers.Benchmarks --configuration Release -- --exporters csv
dotnet run --project Containers.Benchmarks --configuration Release -- --exporters json
```

## Benchmark Parameters

Each benchmark runs with multiple data sizes to show scalability:
- **Small**: 100 elements
- **Medium**: 1,000 elements  
- **Large**: 10,000 elements

## Understanding Results

### Key Metrics
- **Mean**: Average execution time
- **Error**: Half of 99.9% confidence interval
- **StdDev**: Standard deviation of all measurements
- **Allocated**: Memory allocated per operation

### Performance Expectations

**OrderedCollection vs List+Sort:**
- OrderedCollection should be faster for incremental additions
- List+Sort may be faster for bulk operations followed by single sort

**OrderedSet vs HashSet/SortedSet:**
- HashSet will be fastest for pure membership testing
- OrderedSet provides sorted enumeration with competitive performance
- SortedSet uses tree structure vs our list-based approach

**RingBuffer vs Queue/List:**
- RingBuffer should excel in continuous streaming scenarios
- Index access should be much faster than Queue enumeration
- Memory allocation should be more predictable

## Adding New Benchmarks

When implementing new containers, add corresponding benchmark classes:

1. Create `[ContainerName]Benchmarks.cs`
2. Use `[MemoryDiagnoser]` and `[SimpleJob]` attributes
3. Include `[Params]` for different data sizes
4. Compare against relevant built-in collections
5. Test core operations and realistic usage patterns

## Example Output

```
BenchmarkDotNet=v0.14.0, OS=Windows 11
Intel Core i7-12700K, 1 CPU, 20 logical and 12 physical cores
.NET 9.0.0, X64 RyuJIT AVX2

|                    Method | ElementCount |      Mean |     Error |    StdDev |    Allocated |
|-------------------------- |------------- |----------:|----------:|----------:|-------------:|
|     OrderedCollection_Add |          100 |  12.34 μs |  0.123 μs |  0.115 μs |      2.34 KB |
|          List_AddAndSort |          100 |  15.67 μs |  0.234 μs |  0.219 μs |      3.45 KB |
|          SortedList_Add |          100 |  18.90 μs |  0.345 μs |  0.323 μs |      4.56 KB |
```

This helps identify the best container for specific use cases and validates our implementation performance. 
