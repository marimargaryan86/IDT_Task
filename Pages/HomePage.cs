using Allure.Net.Commons.Attributes;
using IDT_Task.Support;
using Microsoft.Playwright;

namespace IDT_Task.Pages;

public class HomePage : BasePage
{
    public HomePage(IPage page) : base(page) { }

    public ILocator HeroHeading =>
        Page.Locator("#slider-carousel h2", new() { HasText = "Full-Fledged practice website for Automation Engineers" }).First;

    [AllureStep("Open the home page")]
    public async Task<HomePage> OpenAsync()
    {
        await Page.GotoAsync(Routes.Home);
        return this;
    }
}
