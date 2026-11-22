using Allure.Net.Commons.Attributes;
using IDT_Task.Components;
using Microsoft.Playwright;

namespace IDT_Task.Pages;

/// <summary>
/// Base for every page: the shared Header and Footer components and the 'scroll up' arrow. No assertions here.
/// Actions that lead to another page return that page, so a test reads as a flow.
/// </summary>
public abstract class BasePage
{
    protected readonly IPage Page;

    protected BasePage(IPage page)
    {
        Page = page;
        Header = new HeaderComponent(page);
        Footer = new FooterComponent(page);
    }

    public HeaderComponent Header { get; }
    public FooterComponent Footer { get; }

    /// <summary>The 'scroll up' arrow at the bottom right. The site's scrollUp plugin adds it to the page and
    /// fades it in only after the page is scrolled down.</summary>
    public ILocator ScrollUpArrow => Page.Locator("#scrollUp");

    [AllureStep("Click the 'scroll up' arrow")]
    public async Task ClickScrollUpArrowAsync() => await ScrollUpArrow.ClickAsync();
}
