namespace IDT_Task.Support;

/// <summary>
/// Run configuration. The values live in one place, the .runsettings TestRunParameters; an environment variable
/// overrides a value for one run. There are no defaults in code: a value missing from both fails the run at
/// start-up with a message naming it. There are no real secrets in this project (the site is public and users
/// are random); if one is ever added, read it from an environment variable only.
/// </summary>
public sealed class Settings
{
    public required string BaseUrl { get; init; }
    public required int ViewportWidth { get; init; }
    public required int ViewportHeight { get; init; }
    public required int ActionTimeoutMs { get; init; }
    public required bool BlockAds { get; init; }

    /// <summary>Reads the settings. The .runsettings values arrive through NUnit's TestContext.Parameters.</summary>
    public static Settings Load() => new()
    {
        BaseUrl = Read("BaseUrl", "IDT_BASE_URL").TrimEnd('/'),
        ViewportWidth = ReadInt("ViewportWidth", "IDT_VIEWPORT_WIDTH"),
        ViewportHeight = ReadInt("ViewportHeight", "IDT_VIEWPORT_HEIGHT"),
        ActionTimeoutMs = ReadInt("ActionTimeoutMs", "IDT_ACTION_TIMEOUT_MS"),
        BlockAds = ReadBool("BlockAds", "IDT_BLOCK_ADS"),
    };

    private static string Read(string parameter, string envVar)
    {
        var fromEnv = Environment.GetEnvironmentVariable(envVar);
        if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv;

        var fromFile = TestContext.Parameters.Get(parameter);
        if (!string.IsNullOrWhiteSpace(fromFile)) return fromFile;

        throw new InvalidOperationException(
            $"Setting '{parameter}' is not set. Run with .runsettings (dotnet test picks it up through the csproj; " +
            $"in an IDE, select it as the run settings file) or set the {envVar} environment variable.");
    }

    private static int ReadInt(string parameter, string envVar)
    {
        var raw = Read(parameter, envVar);
        return int.TryParse(raw, out var value)
            ? value
            : throw new InvalidOperationException($"Setting {parameter}/{envVar} must be a number, got '{raw}'.");
    }

    private static bool ReadBool(string parameter, string envVar)
    {
        var raw = Read(parameter, envVar);
        return bool.TryParse(raw, out var value)
            ? value
            : throw new InvalidOperationException($"Setting {parameter}/{envVar} must be true or false, got '{raw}'.");
    }
}
