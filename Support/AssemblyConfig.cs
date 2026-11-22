// Parallel execution: test classes (fixtures) run at the same time, the tests inside one class run one after
// another. Every test creates its own random user and its own browser context, so classes do not share data.
// Why fixtures and not every test: BaseTest keeps per-test diagnostics in instance fields, and NUnit uses one
// instance per class, so tests of the same class must not overlap.
// The number of parallel workers is NumberOfTestWorkers in .runsettings (1 = run everything in sequence).
[assembly: Parallelizable(ParallelScope.Fixtures)]
