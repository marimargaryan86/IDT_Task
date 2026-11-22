using Allure.Net.Commons.Attributes;
using IDT_Task.Pages;
using IDT_Task.Support;

namespace IDT_Task.Tests;

[TestFixture]
[AllureFeature("Navigation")]
public class ScrollTests : BaseTest
{
    [Test]
    [AllureName("Test Case 25: Verify Scroll Up using 'Arrow' button and Scroll Down functionality")]
    [Description("automationexercise.com Test Case 25")]
    public async Task TC25_ScrollUpUsingArrow_AndScrollDown()
    {
        var home = await new HomePage(Page).OpenAsync();
        await Expect(home.HeroHeading).ToBeVisibleAsync();

        await home.Footer.ScrollIntoViewAsync();
        await Expect(home.Footer.SubscriptionHeading).ToBeInViewportAsync();

        await home.ClickScrollUpArrowAsync();
        await Expect(home.HeroHeading).ToBeInViewportAsync();
    }
}
