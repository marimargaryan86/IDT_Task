namespace IDT_Task.Support;

/// <summary>
/// Run configuration. The values live in one place, the .runsettings TestRunParameters (given by the runner, or
/// read from the file when the IDE does not pass it); an environment variable overrides a value for one run. There are no defaults in code: a value missing from both fails the run at
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

        // Passed in by the test runner (dotnet test does this through the csproj).
        var fromRunner = TestContext.Parameters.Get(parameter);
        if (!string.IsNullOrWhiteSpace(fromRunner)) return fromRunner;

        // Some IDEs (Rider) do not pass .runsettings to the run: read the same file ourselves.
        if (FileParameters.Value.TryGetValue(parameter, out var fromFile) && !string.IsNullOrWhiteSpace(fromFile))
            return fromFile;

        throw new InvalidOperationException(
            $"Setting '{parameter}' is not set. Add it to the TestRunParameters in .runsettings " +
            $"or set the {envVar} environment variable.");
    }

    /// <summary>The TestRunParameters of the project's .runsettings, read directly from the file.</summary>
    private static readonly Lazy<Dictionary<string, string>> FileParameters = new(() =>
    {
        var values = new Dictionary<string, string>();
        // The built dll is in bin/<configuration>/<framework>/; walk up to the folder that has .runsettings.
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var file = Path.Combine(dir.FullName, ".runsettings");
            if (!File.Exists(file)) continue;

            foreach (var p in System.Xml.Linq.XDocument.Load(file).Descendants("TestRunParameters").SelectMany(e => e.Elements("Parameter")))
            {
                var name = (string?)p.Attribute("name");
                var value = (string?)p.Attribute("value");
                if (name != null && value != null) values[name] = value;
            }
            break;
        }
        return values;
    });

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
