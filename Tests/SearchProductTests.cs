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
        // Expected result from the site's own search API. The site matches on category as well as name (not
        // every "dress" result has "dress" in its name), so the term alone cannot say which products belong.
        await using var api = await CreateProductApiAsync();
        var expected = await api.SearchProductNamesAsync(TestData.SearchTerm);

        var home = await new HomePage(Page).OpenAsync();
        await Expect(home.HeroHeading).ToBeVisibleAsync();

        var products = await home.Header.ClickProductsAsync();
        await Expect(products.AllProductsHeading).ToBeVisibleAsync();

        products = await products.SearchAsync(TestData.SearchTerm);
        await Expect(products.SearchedProductsHeading).ToBeVisibleAsync();

        // "All the products related to search are visible": the page lists exactly the products the API
        // returns, in the same order, and every one of them is visible. The first check also fails if the search
        // found nothing, so an empty API answer cannot pass by matching an empty page.
        await Expect(products.ProductCards.First).ToBeVisibleAsync();
        await Expect(products.ProductCards).ToHaveCountAsync(expected.Count);
        await Expect(products.ProductNames).ToHaveTextAsync(expected);
        for (var i = 0; i < expected.Count; i++)
        {
            await Expect(products.ProductCards.Nth(i)).ToBeVisibleAsync();
        }
    }
}
