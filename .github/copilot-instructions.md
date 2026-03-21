# Copilot Instructions for ObjectLayoutInspector

## Definition of Done

A change is considered done only when:

- The solution builds with **zero errors and zero warnings** across all target frameworks.
- All tests pass on all target frameworks (net48, net8.0, net10.0).
- Never create a PR that has build warnings.

## Build & Test Commands

- Build: `dotnet build src/ObjectLayoutInspector.slnx`
- Test all frameworks: `dotnet test src/ObjectLayoutInspector.Tests`
- Test specific framework: `dotnet test src/ObjectLayoutInspector.Tests -f net10.0`

## Project Structure

- `src/ObjectLayoutInspector/` — main library (netstandard2.0, netstandard2.1)
- `src/ObjectLayoutInspector.TestData/` — shared test data types (Objects/, Structs/) used by both tests and benchmarks. No test framework dependencies.
- `src/ObjectLayoutInspector.Tests/` — NUnit tests (net48, net8.0, net10.0)
- `src/ObjectLayoutInspector.Benchmarks/` — BenchmarkDotNet benchmarks (net10.0)

## Conventions

- Test data types (structs/classes used as layout subjects) go in `ObjectLayoutInspector.TestData`, not in the test project.
- Framework-specific test files use the naming convention `*.Net48.cs` (net48 only) and `*.NetCore.cs` (net8.0/net10.0 only).
- All layout examples and sizes in documentation assume x64 (64-bit) runtime.
