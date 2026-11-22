using Allure.Net.Commons.Attributes;
using Microsoft.Playwright;

namespace IDT_Task.Components;

/// <summary>The page footer, present on every page.</summary>
public class FooterComponent
{
    private readonly IPage _page;

    public FooterComponent(IPage page) => _page = page;

    private ILocator Root => _page.Locator("footer");

    public ILocator SubscriptionHeading => Root.GetByRole(AriaRole.Heading, new() { Name = "Subscription" });

    [AllureStep("Scroll to the footer")]
    public async Task ScrollIntoViewAsync() => await Root.ScrollIntoViewIfNeededAsync();
}
