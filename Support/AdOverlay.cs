using Microsoft.Playwright;

namespace IDT_Task.Support;

/// <summary>
/// Removes Google's full-screen "vignette" ad if one ever slips past the request blocking in
/// <see cref="BaseTest"/>. It is a Playwright locator handler: Playwright runs it automatically whenever the
/// overlay is visible and would block a click, so no test or page object has to know about ads.
/// The selectors follow Google's published ad markup; no run on this site has shown the overlay yet, so they
/// were only checked once against a fake overlay (the test was removed), not against a real Google ad.
/// </summary>
public static class AdOverlay
{
    // Vignette: an <ins class="adsbygoogle ..." data-vignette-loaded="true"> element covering the page.
    private const string OverlaySelector =
        "ins.adsbygoogle[data-vignette-loaded='true'], ins.adsbygoogle-noablate[data-vignette-loaded]";

    private const string Hash = "#google_vignette";

    public static async Task InstallAsync(IPage page)
    {
        await page.AddLocatorHandlerAsync(page.Locator(OverlaySelector), async overlay =>
        {
            await overlay.EvaluateAllAsync<bool>("elements => { elements.forEach(e => e.remove()); return true; }");

            // The ad adds #google_vignette to the address; strip it so the page URL is the real one again.
            if (page.Url.Contains(Hash, StringComparison.Ordinal))
                await page.EvaluateAsync("() => history.replaceState(null, '', location.pathname + location.search)");
        });
    }
}
