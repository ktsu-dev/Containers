# Copyright (c) ktsu.dev
# All rights reserved.
# Licensed under the MIT license.

<#
.SYNOPSIS
    Runs ktsu.Containers performance benchmarks.

.DESCRIPTION
    This script provides easy access to run various benchmark scenarios for the ktsu.Containers library.
    It uses BenchmarkDotNet to measure performance and memory usage.

.PARAMETER Target
    Specifies which benchmarks to run. Valid values: All, OrderedCollection, OrderedSet, RingBuffer, Standard, CrossComparison, PerformanceAnalysis, Quick

.PARAMETER Export
    Export format for results. Valid values: None, Html, Csv, Json, All

.PARAMETER Filter
    Custom filter pattern for benchmark methods (e.g., "*Add*", "*Contains*")

.EXAMPLE
    .\run-benchmarks.ps1 -Target All
    Runs all benchmarks

.EXAMPLE
    .\run-benchmarks.ps1 -Target CrossComparison -Export Html
    Runs cross-collection comparison benchmarks and exports to HTML

.EXAMPLE
    .\run-benchmarks.ps1 -Filter "*Add*" -Export Csv
    Runs only Add-related benchmarks and exports to CSV
#>

param(
    [ValidateSet("All", "OrderedCollection", "OrderedSet", "RingBuffer", "Standard", "CrossComparison", "PerformanceAnalysis", "Quick")]
    [string]$Target = "All",

    [ValidateSet("None", "Html", "Csv", "Json", "All")]
    [string]$Export = "None",

    [string]$Filter = ""
)

# Ensure we're in the correct directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
Set-Location $rootDir

Write-Host "ktsu.Containers Benchmark Runner" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Build the project first
Write-Host "Building benchmark project..." -ForegroundColor Yellow
$buildResult = dotnet build Containers.Benchmarks --configuration Release --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Please fix compilation errors first."
    exit 1
}
Write-Host "Build successful!" -ForegroundColor Green
Write-Host ""

# Prepare benchmark arguments
$benchmarkArgs = @("run", "--project", "Containers.Benchmarks", "--configuration", "Release")

# Add filter based on target
switch ($Target) {
    "OrderedCollection" {
        $benchmarkArgs += "--filter"
        $benchmarkArgs += "*OrderedCollectionBenchmarks*"
        Write-Host "Running OrderedCollection benchmarks..." -ForegroundColor Yellow
        Write-Host "This compares OrderedCollection<T> against List<T> + Sort and SortedList<T>" -ForegroundColor Gray
    }
    "OrderedSet" {
        $benchmarkArgs += "--filter"
        $benchmarkArgs += "*OrderedSetBenchmarks*"
        Write-Host "Running OrderedSet benchmarks..." -ForegroundColor Yellow
        Write-Host "This compares OrderedSet<T> against HashSet<T> and SortedSet<T>" -ForegroundColor Gray
    }
    "RingBuffer" {
        $benchmarkArgs += "--filter"
        $benchmarkArgs += "*RingBufferBenchmarks*"
        Write-Host "Running RingBuffer benchmarks..." -ForegroundColor Yellow
        Write-Host "This compares RingBuffer<T> against Queue<T> and List<T> with size limiting" -ForegroundColor Gray
    }
    "Standard" {
        $benchmarkArgs += "--filter"
        $benchmarkArgs += "*StandardCollectionBenchmarks*"
        Write-Host "Running Standard .NET Collection benchmarks..." -ForegroundColor Yellow
        Write-Host "This establishes baseline performance for built-in .NET collections" -ForegroundColor Gray
    }
    "CrossComparison" {
        $benchmarkArgs += "--filter"
        $benchmarkArgs += "*CrossCollectionComparisonBenchmarks*"
        Write-Host "Running Cross-Collection Comparison benchmarks..." -ForegroundColor Yellow
        Write-Host "This provides direct comparisons across different collection types for common scenarios" -ForegroundColor Gray
    }
    "PerformanceAnalysis" {
        $benchmarkArgs += "--filter"
        $benchmarkArgs += "*PerformanceAnalysisBenchmarks*"
        Write-Host "Running Performance Analysis benchmarks..." -ForegroundColor Yellow
        Write-Host "This focuses on scalability, edge cases, and specialized scenarios" -ForegroundColor Gray
    }
    "Quick" {
        # Run a subset for quick testing
        $benchmarkArgs += "--filter"
        $benchmarkArgs += "*Add*"
        $benchmarkArgs += "--job"
        $benchmarkArgs += "short"
        Write-Host "Running quick benchmark subset (Add operations with short job)..." -ForegroundColor Yellow
        Write-Host "This provides fast feedback for development" -ForegroundColor Gray
    }
    "All" {
        Write-Host "Running all benchmarks..." -ForegroundColor Yellow
        Write-Host "This includes container-specific, standard collection, cross-comparison, and performance analysis benchmarks" -ForegroundColor Gray
        Write-Host "Warning: This will take a significant amount of time!" -ForegroundColor Red
    }
}

# Add custom filter if provided
if ($Filter -ne "") {
    $benchmarkArgs += "--filter"
    $benchmarkArgs += $Filter
    Write-Host "Using custom filter: $Filter" -ForegroundColor Yellow
}

# Add export options
if ($Export -ne "None") {
    $benchmarkArgs += "--"

    switch ($Export) {
        "Html" { $benchmarkArgs += "--exporters"; $benchmarkArgs += "html" }
        "Csv" { $benchmarkArgs += "--exporters"; $benchmarkArgs += "csv" }
        "Json" { $benchmarkArgs += "--exporters"; $benchmarkArgs += "json" }
        "All" { $benchmarkArgs += "--exporters"; $benchmarkArgs += "html,csv,json" }
    }

    Write-Host "Results will be exported as: $Export" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Available Benchmark Categories:" -ForegroundColor Cyan
Write-Host "------------------------------" -ForegroundColor Cyan
Write-Host "• OrderedCollection: Compare our ordered collection against List<T> + Sort" -ForegroundColor Gray
Write-Host "• OrderedSet: Compare our ordered set against HashSet<T> and SortedSet<T>" -ForegroundColor Gray
Write-Host "• RingBuffer: Compare our ring buffer against Queue<T> and List<T>" -ForegroundColor Gray
Write-Host "• Standard: Baseline performance of built-in .NET collections" -ForegroundColor Gray
Write-Host "• CrossComparison: Direct comparisons across different collection types" -ForegroundColor Gray
Write-Host "• PerformanceAnalysis: Scalability and edge case analysis" -ForegroundColor Gray
Write-Host "• Quick: Fast subset for development feedback" -ForegroundColor Gray
Write-Host ""

Write-Host "Starting benchmarks..." -ForegroundColor Green
Write-Host "This may take several minutes depending on the scope." -ForegroundColor Gray
Write-Host ""

# Run the benchmarks
$startTime = Get-Date
& dotnet @benchmarkArgs

$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host ""
if ($LASTEXITCODE -eq 0) {
    Write-Host "Benchmarks completed successfully!" -ForegroundColor Green
    Write-Host "Total duration: $($duration.ToString('mm\:ss'))" -ForegroundColor Gray

    if ($Export -ne "None") {
        Write-Host ""
        Write-Host "Results exported to BenchmarkDotNet.Artifacts folder:" -ForegroundColor Cyan

        $artifactsPath = Join-Path $rootDir "Containers.Benchmarks\BenchmarkDotNet.Artifacts"
        if (Test-Path $artifactsPath) {
            Get-ChildItem $artifactsPath -Recurse -File | Where-Object {
                $_.Extension -in @('.html', '.csv', '.json')
            } | ForEach-Object {
                Write-Host "  - $($_.FullName)" -ForegroundColor Gray
            }
        }
    }

    Write-Host ""
    Write-Host "Performance Analysis Tips:" -ForegroundColor Cyan
    Write-Host "-------------------------" -ForegroundColor Cyan
    Write-Host "• Look for Mean execution time as the primary metric" -ForegroundColor Gray
    Write-Host "• Check Allocated memory for memory efficiency" -ForegroundColor Gray
    Write-Host "• Compare performance across different ElementCount parameters" -ForegroundColor Gray
    Write-Host "• Consider use case when interpreting results:" -ForegroundColor Gray
    Write-Host "  - OrderedCollection: Best for incremental sorted insertions" -ForegroundColor Gray
    Write-Host "  - OrderedSet: Best for unique sorted collections" -ForegroundColor Gray
    Write-Host "  - RingBuffer: Best for fixed-size circular buffers" -ForegroundColor Gray
    Write-Host ""
    Write-Host "• Red flags to watch for:" -ForegroundColor Yellow
    Write-Host "  - Unexpectedly high memory allocations" -ForegroundColor Gray
    Write-Host "  - Non-linear performance scaling where linear expected" -ForegroundColor Gray
    Write-Host "  - Performance significantly worse than .NET equivalents" -ForegroundColor Gray

} else {
    Write-Error "Benchmarks failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "----------" -ForegroundColor Cyan
Write-Host "• Run specific benchmark categories for focused analysis" -ForegroundColor Gray
Write-Host "• Export results to HTML for detailed analysis and sharing" -ForegroundColor Gray
Write-Host "• Use CrossComparison benchmarks to understand trade-offs" -ForegroundColor Gray
Write-Host "• Use PerformanceAnalysis for scalability validation" -ForegroundColor Gray
Write-Host "• Compare results across different machines and configurations" -ForegroundColor Gray
