namespace IDT_Task.Support;

/// <summary>
/// Run configuration. The values live in the TestRunParameters of .runsettings, which the runner hands to NUnit; an
/// environment variable overrides one for a single run. There are no defaults in code: a missing value stops the
/// run at start-up with a message naming it.
/// </summary>
public sealed class Settings
{
    public required string BaseUrl { get; init; }
    public required int ActionTimeoutMs { get; init; }
    public required int RetryAttempts { get; init; }

    public static Settings Load() => new()
    {
        BaseUrl = Read("BaseUrl", "IDT_BASE_URL").TrimEnd('/'),
        ActionTimeoutMs = ReadInt("ActionTimeoutMs", "IDT_ACTION_TIMEOUT_MS"),
        RetryAttempts = ReadInt("RetryAttempts", "IDT_RETRY_ATTEMPTS"),
    };

    private static string Read(string parameter, string envVar)
    {
        var value = Environment.GetEnvironmentVariable(envVar);
        if (string.IsNullOrWhiteSpace(value)) value = TestContext.Parameters.Get(parameter);
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException(
                $"Setting '{parameter}' is not set. Add it to the TestRunParameters in .runsettings " +
                $"(in an IDE, select that file as the run settings file) or set the {envVar} environment variable.");
    }

    private static int ReadInt(string parameter, string envVar)
    {
        var raw = Read(parameter, envVar);
        return int.TryParse(raw, out var value) && value >= 0
            ? value
            : throw new InvalidOperationException($"Setting {parameter}/{envVar} must be a number, 0 or more, got '{raw}'.");
    }
}
