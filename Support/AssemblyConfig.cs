// Test classes run in parallel when NumberOfTestWorkers (.runsettings) is above 1; the tests inside a class
// never overlap, because NUnit uses one instance per class. Each test has its own user and browser context.
[assembly: Parallelizable(ParallelScope.Fixtures)]
