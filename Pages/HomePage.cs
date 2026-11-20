using Microsoft.Playwright;

namespace IDT_Task.Pages;

public class HomePage : BasePage
{
    public HomePage(IPage page) : base(page) { }

    public ILocator HeroHeading =>
        Page.Locator("#slider-carousel h2", new() { HasText = "Full-Fledged practice website for Automation Engineers" }).First;

    public async Task OpenAsync() => await Page.GotoAsync("/");
}
