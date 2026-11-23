using Microsoft.Playwright;

namespace IDT_Task.Support;

/// <summary>Blocks ad and tracker requests: the site's ads can cover the page and steal clicks.</summary>
public static class AdBlocking
{
    private static readonly string[] BlockedHosts =
    {
        "googlesyndication.com", "doubleclick.net", "googleadservices.com",
        "google-analytics.com", "googletagmanager.com", "adservice.google.",
        "pagead2.", "adsbygoogle", "amazon-adsystem.com", "adnxs.com"
    };

    public static bool IsBlocked(string url) =>
        BlockedHosts.Any(h => url.Contains(h, StringComparison.OrdinalIgnoreCase));

    public static Task InstallAsync(IBrowserContext context) =>
        context.RouteAsync("**/*", async route =>
        {
            if (IsBlocked(route.Request.Url))
                await route.AbortAsync();
            else
                await route.ContinueAsync();
        });
}
