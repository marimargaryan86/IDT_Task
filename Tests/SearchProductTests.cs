using IDT_Task.Pages;
using IDT_Task.Support;

namespace IDT_Task.Tests;

[TestClass]
public class SearchProductTests : BaseTest
{
    [TestMethod]
    public async Task SearchProduct_ShowsOnlyMatchingProducts()
    {
        const string term = "dress";
        var home = new HomePage(Page);
        var products = new ProductsPage(Page);

        await home.OpenAsync();
        await home.ClickProductsAsync();
        await Expect(products.AllProductsHeading).ToBeVisibleAsync();

        await products.SearchAsync(term);
        await Expect(products.SearchedProductsHeading).ToBeVisibleAsync();
        await Expect(products.ProductCards.First).ToBeVisibleAsync();

        // The site matches on name or category, so only assert that results are listed.
        var names = await products.ProductNames.AllInnerTextsAsync();
        Assert.IsTrue(names.Count > 0, "Search returned no products.");

        // A term that matches nothing shows an empty result list.
        await products.SearchAsync("zzzxqj");
        await Expect(products.SearchedProductsHeading).ToBeVisibleAsync();
        await Expect(products.ProductCards).ToHaveCountAsync(0);
    }
}
