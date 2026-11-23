using System.Collections.Concurrent;
using Allure.Net.Commons;
using Microsoft.Playwright;

namespace IDT_Task.Support;

/// <summary>
/// Evidence for one test: a light Playwright trace, plus a log of failed requests, HTTP errors and console errors.
/// When the test fails, the screenshot, the trace and the log are saved under TestResults/ and attached to the
/// Allure report; when it passes, the trace is discarded.
/// </summary>
public sealed class FailureEvidence
{
    // Filled from Playwright's event callbacks, which can run on other threads.
    private readonly ConcurrentQueue<string> _log = new();

    public async Task StartAsync(IBrowserContext context, IPage page)
    {
        // Screenshots, actions and network only. Snapshots (page DOM) and Sources (test code) are off to keep
        // tracing cheap on every test, including the ones that pass and are discarded.
        await context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = false, Sources = false });

        // Requests blocked on purpose are not reported.
        page.RequestFailed += (_, r) =>
        {
            if (!AdBlocking.IsBlocked(r.Url))
                _log.Enqueue($"{Stamp()} REQUEST FAILED {r.Method} {r.Url} ({r.Failure})");
        };
        page.Response += (_, r) =>
        {
            if (r.Status >= 400)
                _log.Enqueue($"{Stamp()} HTTP {r.Status} {r.Request.Method} {r.Url}");
        };
        page.Console += (_, m) =>
        {
            if (m.Type == "error")
                _log.Enqueue($"{Stamp()} CONSOLE ERROR {m.Text}");
        };
        page.PageError += (_, e) => _log.Enqueue($"{Stamp()} PAGE ERROR {e}");
    }

    public async Task CollectAsync(IBrowserContext? context, IPage? page)
    {
        // If the browser never started (launch failed), there is nothing to collect and no context to stop;
        // returning here keeps the original setup error visible instead of a NullReferenceException.
        if (context is null || page is null) return;

        var failed = TestContext.CurrentContext.Result.Outcome.Status != NUnit.Framework.Interfaces.TestStatus.Passed;
        if (!failed)
        {
            await context.Tracing.StopAsync(); // discard
            return;
        }

        var dir = Path.GetFullPath("TestResults");
        Directory.CreateDirectory(Path.Combine(dir, "traces"));
        Directory.CreateDirectory(Path.Combine(dir, "screenshots"));
        var name = TestContext.CurrentContext.Test.Name;
        var stamp = DateTime.Now.ToString("HHmmss");

        var screenshot = Path.Combine(dir, "screenshots", $"{name}-{stamp}.png");
        var trace = Path.Combine(dir, "traces", $"{name}-{stamp}.zip");
        await page.ScreenshotAsync(new() { Path = screenshot });
        await context.Tracing.StopAsync(new() { Path = trace });

        // The URL and the log reach Allure on their own, as the "Console Output" attachment, because they are
        // written with TestContext.Out.
        TestContext.Out.WriteLine($"URL at failure: {page.Url}{Environment.NewLine}Trace: {trace}{Environment.NewLine}" +
                                  string.Join(Environment.NewLine, _log));
        AllureApi.AddAttachment("Screenshot at failure", "image/png", screenshot);
        AllureApi.AddAttachment("Playwright trace (open with show-trace)", "application/zip", trace);
    }

    private static string Stamp() => DateTime.Now.ToString("HH:mm:ss.fff");
}
