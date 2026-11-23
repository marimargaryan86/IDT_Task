using Allure.Net.Commons;
using Allure.NUnit;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace IDT_Task.Support;

/// <summary>
/// Shared setup: base URL and viewport from <see cref="Settings"/>, optional blocking of ad/tracker requests
/// (ads overlay the site and steal clicks), and failure evidence (trace zip, screenshot, failed requests,
/// console errors), which is also attached to the Allure report. Browser, headless and slow-motion come from
/// .runsettings, or BROWSER / HEADED. Test users made with <see cref="NewTestUser"/> or
/// <see cref="CreateTestUserViaApiAsync"/> are deleted through the API after every test.
/// [AllureNUnit] on this class applies to every test class that inherits it.
/// </summary>
[AllureNUnit]
public abstract class BaseTest : PageTest
{
    private static readonly string[] BlockedHosts =
    {
        "googlesyndication.com", "doubleclick.net", "googleadservices.com",
        "google-analytics.com", "googletagmanager.com", "adservice.google.",
        "pagead2.", "adsbygoogle", "amazon-adsystem.com", "adnxs.com"
    };

    private readonly List<string> _diagnostics = new();
    private readonly List<User> _testUsers = new();
    private Settings? _config;

    protected Settings Config => _config ??= Settings.Load();

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = Config.BaseUrl,
        ViewportSize = new ViewportSize { Width = Config.ViewportWidth, Height = Config.ViewportHeight },
    };

    /// <summary>An API client for creating and deleting users, pointed at the configured site.</summary>
    protected Task<AccountApi> CreateAccountApiAsync() => AccountApi.CreateAsync(Playwright, Config.BaseUrl);

    /// <summary>An API client for reading products, pointed at the configured site.</summary>
    protected Task<ProductApi> CreateProductApiAsync() => ProductApi.CreateAsync(Playwright, Config.BaseUrl);

    /// <summary>
    /// A new random user, not yet on the site (for a test that signs up through the UI). It is deleted after
    /// the test if the test created it and did not delete it.
    /// </summary>
    protected User NewTestUser()
    {
        var user = TestData.RandomUser();
        _testUsers.Add(user);
        return user;
    }

    /// <summary>A new random user, already created on the site through the API. It is deleted after the test.</summary>
    protected async Task<User> CreateTestUserViaApiAsync()
    {
        var user = NewTestUser(); // registered for cleanup first, so a half-finished create is still cleaned up
        await using var api = await CreateAccountApiAsync();
        await api.CreateAccountAsync(user);
        return user;
    }

    [SetUp]
    public async Task SetUpPage()
    {
        // One instance serves every test of a class, so start each test empty.
        _diagnostics.Clear();
        _testUsers.Clear();

        Page.SetDefaultTimeout(Config.ActionTimeoutMs);
        Page.SetDefaultNavigationTimeout(Config.ActionTimeoutMs);

        // Light tracing: screenshots, actions and network only. Snapshots (page DOM) and Sources (test code) are
        // off to keep tracing cheap on every test, including the ones that pass and are discarded.
        await Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = false, Sources = false });

        if (Config.BlockAds)
        {
            await Context.RouteAsync("**/*", async route =>
            {
                if (IsBlocked(route.Request.Url))
                    await route.AbortAsync();
                else
                    await route.ContinueAsync();
            });
        }

        // Evidence for failures. Requests we block on purpose are not reported.
        Page.RequestFailed += (_, r) =>
        {
            if (!IsBlocked(r.Url))
                _diagnostics.Add($"{Stamp()} REQUEST FAILED {r.Method} {r.Url} ({r.Failure})");
        };
        Page.Response += (_, r) =>
        {
            if (r.Status >= 400)
                _diagnostics.Add($"{Stamp()} HTTP {r.Status} {r.Request.Method} {r.Url}");
        };
        Page.Console += (_, m) =>
        {
            if (m.Type == "error")
                _diagnostics.Add($"{Stamp()} CONSOLE ERROR {m.Text}");
        };
        Page.PageError += (_, e) => _diagnostics.Add($"{Stamp()} PAGE ERROR {e}");
    }

    /// <summary>One teardown, so the order is fixed: evidence first (while the page still shows the failure),
    /// then the test users are removed, even if collecting evidence failed.</summary>
    [TearDown]
    public async Task TearDownTest()
    {
        try
        {
            await CollectEvidenceAsync();
        }
        finally
        {
            await DeleteTestUsersAsync();
        }
    }

    /// <summary>
    /// Deletes this test's users through the API. "Account not found" is fine: the test already deleted the user
    /// in the UI, or never got as far as creating it. A failure here is logged, not thrown, so it never replaces
    /// the test's own result.
    /// </summary>
    private async Task DeleteTestUsersAsync()
    {
        if (_testUsers.Count == 0) return;
        try
        {
            await using var api = await CreateAccountApiAsync();
            foreach (var user in _testUsers)
            {
                try
                {
                    await api.DeleteAccountAsync(user, allowMissing: true);
                }
                catch (Exception ex)
                {
                    TestContext.Out.WriteLine($"Cleanup failed, account {user.Email} may be left on the site: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Cleanup failed, could not reach the API: {ex.Message}");
        }
        finally
        {
            _testUsers.Clear();
        }
    }

    private async Task CollectEvidenceAsync()
    {
        // If the browser never started (launch failed), there is nothing to collect and no context to stop;
        // returning here keeps the original setup error visible instead of a NullReferenceException.
        if (Context is null || Page is null) return;

        var failed = TestContext.CurrentContext.Result.Outcome.Status != NUnit.Framework.Interfaces.TestStatus.Passed;
        if (!failed)
        {
            await Context.Tracing.StopAsync(); // discard
            return;
        }

        var dir = Path.GetFullPath("TestResults");
        Directory.CreateDirectory(Path.Combine(dir, "traces"));
        Directory.CreateDirectory(Path.Combine(dir, "screenshots"));
        var name = TestContext.CurrentContext.Test.Name;
        var stamp = DateTime.Now.ToString("HHmmss");

        await Page.ScreenshotAsync(new() { Path = Path.Combine(dir, "screenshots", $"{name}-{stamp}.png") });
        var trace = Path.Combine(dir, "traces", $"{name}-{stamp}.zip");
        await Context.Tracing.StopAsync(new() { Path = trace });

        var report = $"URL at failure: {Page.Url}{Environment.NewLine}Trace: {trace}{Environment.NewLine}" +
                     string.Join(Environment.NewLine, _diagnostics);
        TestContext.Out.WriteLine(report);

        // Same evidence in the Allure report, next to the failed step.
        AllureApi.AddAttachment("Screenshot at failure", "image/png",
            Path.Combine(dir, "screenshots", $"{name}-{stamp}.png"));
        AllureApi.AddAttachment("Playwright trace (open with show-trace)", "application/zip", trace);
        // The URL and the HTTP / console errors (the `report` text above) reach Allure on their own, as the
        // "Console Output" attachment, because they are written with TestContext.Out.
    }

    private static bool IsBlocked(string url) =>
        BlockedHosts.Any(h => url.Contains(h, StringComparison.OrdinalIgnoreCase));

    private static string Stamp() => DateTime.Now.ToString("HH:mm:ss.fff");
}
