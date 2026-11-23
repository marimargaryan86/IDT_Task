using Microsoft.Playwright;

namespace IDT_Task.Support;

/// <summary>
/// Sits between the browser and the network. Ad and tracker requests are blocked (ads can cover the page and steal
/// clicks). A GET page load of the site that answers 5xx is sent again up to <see cref="Settings.RetryAttempts"/>
/// times and the retry is reported through <paramref name="onRetry"/>. Forms (POST), scripts and images are never
/// retried, so nothing is sent twice.
/// </summary>
public sealed class RequestRouting
{
    private static readonly string[] BlockedHosts =
    {
        "googlesyndication.com", "doubleclick.net", "googleadservices.com",
        "google-analytics.com", "googletagmanager.com", "adservice.google.",
        "pagead2.", "adsbygoogle", "amazon-adsystem.com", "adnxs.com"
    };

    private readonly Settings _config;
    private readonly Action<string> _onRetry;

    public RequestRouting(Settings config, Action<string> onRetry)
    {
        _config = config;
        _onRetry = onRetry;
    }

    public static bool IsBlocked(string url) =>
        BlockedHosts.Any(h => url.Contains(h, StringComparison.OrdinalIgnoreCase));

    public Task InstallAsync(IBrowserContext context) => context.RouteAsync("**/*", HandleAsync);

    private async Task HandleAsync(IRoute route)
    {
        var r = route.Request;
        if (IsBlocked(r.Url))
        {
            await route.AbortAsync();
            return;
        }

        if (_config.RetryAttempts == 0 || r.Method != "GET" || r.ResourceType != "document" || !IsSite(r.Url))
        {
            await route.ContinueAsync();
            return;
        }

        for (var attempt = 0; ; attempt++)
        {
            // MaxRedirects = 0: a redirect goes back to the browser, which follows it as a new (routed) request.
            var response = await route.FetchAsync(new() { MaxRedirects = 0 });
            if (response.Status < 500 || attempt == _config.RetryAttempts)
            {
                await route.FulfillAsync(new() { Response = response });
                return;
            }

            _onRetry($"GET {r.Url}: HTTP {response.Status}, retry {attempt + 1}/{_config.RetryAttempts}");
            await Task.Delay(1000 * (attempt + 1)); // back off before the next try
        }
    }

    /// <summary>True for requests to the site under test (same host as the base URL), not third parties.</summary>
    private bool IsSite(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var u) &&
        string.Equals(u.Host, new Uri(_config.BaseUrl).Host, StringComparison.OrdinalIgnoreCase);
}
