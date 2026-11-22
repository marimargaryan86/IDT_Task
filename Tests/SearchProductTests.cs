using Allure.Net.Commons.Attributes;
using IDT_Task.Pages;
using IDT_Task.Support;

namespace IDT_Task.Tests;

[TestFixture]
[AllureFeature("Catalog")]
public class SearchProductTests : BaseTest
{
    [Test]
    [AllureName("Test Case 9: Search Product")]
    [Description("automationexercise.com Test Case 9")]
    public async Task TC09_SearchProduct()
    {
        var home = await new HomePage(Page).OpenAsync();
        await Expect(home.HeroHeading).ToBeVisibleAsync();

        var products = await home.Header.ClickProductsAsync();
        await Expect(products.AllProductsHeading).ToBeVisibleAsync();

        products = await products.SearchAsync(TestData.SearchTerm);
        await Expect(products.SearchedProductsHeading).ToBeVisibleAsync();

        // "All the products related to search are visible": at least one result, and every result card is
        // visible. The site matches on name or category, so the names are not checked against the term.
        await Expect(products.ProductCards.First).ToBeVisibleAsync();
        var count = await products.ProductCards.CountAsync();
        for (var i = 0; i < count; i++)
        {
            await Expect(products.ProductCards.Nth(i)).ToBeVisibleAsync();
        }
    }
}
