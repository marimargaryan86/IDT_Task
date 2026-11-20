using IDT_Task.Pages;
using IDT_Task.Support;

namespace IDT_Task.Tests;

[TestClass]
public class ScrollTests : BaseTest
{
    [TestMethod]
    public async Task ScrollUp_WithoutArrowButton()
    {
        var home = new HomePage(Page);

        await home.OpenAsync();
        await home.ScrollToFooterAsync();
        await Expect(home.SubscriptionHeading).ToBeVisibleAsync();

        // No click on the 'scroll up' arrow: use the page's own scroll.
        await home.ScrollToTopAsync();
        await Expect(home.HeroHeading).ToBeInViewportAsync();
    }

    [TestMethod]
    public async Task ScrollDown_ShowsSubscription()
    {
        var home = new HomePage(Page);

        await home.OpenAsync();
        await Expect(home.HeroHeading).ToBeInViewportAsync();

        await home.ScrollToFooterAsync();
        await Expect(home.SubscriptionHeading).ToBeInViewportAsync();
    }
}
