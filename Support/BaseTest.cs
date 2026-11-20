using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace IDT_Task.Support;

/// <summary>
/// Shared setup: base URL and blocking of ad/tracker requests (ads overlay the site and steal clicks).
/// Headed run: HEADED=1 dotnet test
/// </summary>
public abstract class BaseTest : PageTest
{
    public const string BaseUrl = "https://automationexercise.com";

    private static readonly string[] BlockedHosts =
    {
        "googlesyndication.com", "doubleclick.net", "googleadservices.com",
        "google-analytics.com", "googletagmanager.com", "adservice.google.",
        "pagead2.", "adsbygoogle", "amazon-adsystem.com", "adnxs.com"
    };

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = BaseUrl,
        ViewportSize = new ViewportSize { Width = 1440, Height = 900 },
    };

    [TestCleanup]
    public async Task ScreenshotOnFailure()
    {
        if (TestContext.CurrentTestOutcome == UnitTestOutcome.Passed) return;
        Directory.CreateDirectory("TestResults/screenshots");
        await Page.ScreenshotAsync(new() { Path = $"TestResults/screenshots/{TestContext.TestName}.png" });
        Console.WriteLine($"URL at failure: {Page.Url}");
    }

    [TestInitialize]
    public async Task BlockAds()
    {
        await Context.RouteAsync("**/*", async route =>
        {
            var url = route.Request.Url;
            if (BlockedHosts.Any(h => url.Contains(h, StringComparison.OrdinalIgnoreCase)))
                await route.AbortAsync();
            else
                await route.ContinueAsync();
        });
    }
}
