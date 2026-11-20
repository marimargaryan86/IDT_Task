using Microsoft.Playwright;

namespace IDT_Task.Pages;

public class ProductsPage : BasePage
{
    public ProductsPage(IPage page) : base(page) { }

    public ILocator AllProductsHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "All Products" });
    public ILocator SearchedProductsHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "Searched Products" });
    public ILocator ProductCards => Page.Locator(".features_items .product-image-wrapper");
    public ILocator ProductNames => Page.Locator(".features_items .productinfo p");

    private ILocator SearchInput => Page.Locator("#search_product");
    private ILocator SearchButton => Page.Locator("#submit_search");

    public async Task SearchAsync(string term)
    {
        await SearchInput.FillAsync(term);
        await SearchButton.ClickAsync();
    }

    /// <summary>Hover the card (the add button only shows on hover) and add it to the cart.</summary>
    public async Task AddFirstProductToCartAsync()
    {
        var card = ProductCards.First;
        await card.HoverAsync();
        await card.Locator(".product-overlay a.add-to-cart").ClickAsync();
    }

    public async Task ViewCartFromModalAsync() =>
        await Page.Locator("#cartModal").GetByRole(AriaRole.Link, new() { Name = "View Cart" }).ClickAsync();
}
